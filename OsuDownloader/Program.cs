using OsuDownloader.Beatmap;
using OsuDownloader.DataBase;
using OsuDownloader.Exceptions.Local;
using OsuDownloader.Exceptions.Server;
using OsuDownloader.Server;
using OsuDownloader.Server.Mirror;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OsuDownloader
{

    public class Program
    {

        private static BeatmapDb localDb;
        private static BeatmapMirror mirror;

        private static string osuLocation;

        private static string DownloadFolder { get => Path.Combine(osuLocation, "Downloads"); }
        private static string SongsFolder { get => Path.Combine(osuLocation, "Songs"); }

        private static bool preferNoVid = false;

        public const string USER_AGENT = "osu!downloader";

        private static int downloaded = 0;
        private static int downloadCount = 0;

        [STAThread]
        static void Main(string[] args)
        {
            Console.Out.WriteLine($"* 진행전 켜진 오스는 닫아주세요.");

            while (true) {
                try
                {
                    osuLocation = AskLocation("오스가 설치된 폴더를 선택해 주세요");
                    if (osuLocation == null)
                    {
                        Application.Exit();
                        return;
                    }
                    if (isValidInstallation(osuLocation))
                        break;
                    else
                        Console.Out.WriteLine("선택된 폴더는 오스 폴더가 아닙니다.");
                } catch (Exception)
                {
                    Console.Out.WriteLine("잘못 된 경로 입니다");
                }
            }
            Console.Out.WriteLine($"로컬 비트맵 리스트 파싱 중...");
            localDb = ParseDb(osuLocation);
            Console.Out.WriteLine($"로컬 비트맵 리스트 파싱 완료");
            Console.Out.WriteLine($"비트맵 셋 {localDb.BeatmapSets.Count} 개가 발견 되었습니다.");

            Console.Out.WriteLine("----------------------------------------------------");

            Console.Out.WriteLine($"미러 서버에서 비트맵 리스트 받아오는 중...");

            int counter = 1;
            while (true) {
                try
                {
                    mirror = new OsuHexide();
                    Console.Out.WriteLine($"미러 서버 : {mirror.MirrorSite}");
                    Console.Out.WriteLine($"비트맵 셋 {mirror.BeatmapSetCount} 개가 발견되었습니다.");
                    break;
                } catch (Exception e)
                {
                    Console.Out.WriteLine($"미러 서버 접속이 실패 했습니다 {counter} 초 뒤 재 시도 합니다 {e}");
                    System.Threading.Thread.Sleep(counter * 1000);
                    counter *= 2;
                }
            }

            Console.Out.WriteLine("----------------------------------------------------");

            BeatmapSearcher localSearcher = new BeatmapSearcher(localDb);
            BeatmapSearcher mirrorSearcher = new BeatmapSearcher(mirror);

            Console.Out.WriteLine("다운로드 받을 비트맵 셋 제목 검색 키워드를 입력해주세요 (정규식 사용 가능)");

            string keyword = Console.In.ReadLine();
            localSearcher.TitleKeyword = mirrorSearcher.TitleKeyword = keyword;

            List<IBeatmapSet> havingBeatmapSets = localSearcher.Search();
            List<IBeatmapSet> queuedBeatmapSets = mirrorSearcher.Search();

            downloadCount = queuedBeatmapSets.Count - havingBeatmapSets.Count;

            Parallel.ForEach(havingBeatmapSets, (IBeatmapSet set) => queuedBeatmapSets.Remove(set));

            Console.Out.WriteLine($"비트맵에 동영상을 포함할까요? (Y / N)");

            if (Console.In.ReadLine().ToLower().Equals("y"))
            {
                Console.Out.WriteLine($"예상 용량 {downloadCount * 7.741} MB");
                preferNoVid = true;
            }
            else
            {
                Console.Out.WriteLine($"예상 용량 {downloadCount * 14.273} MB");
            }

            Console.Out.WriteLine($"비트맵 셋 {downloadCount} 개를 다운로드 합니다");

            Console.Out.WriteLine("----------------------------------------------------");

            if (!Directory.Exists(SongsFolder)) Directory.CreateDirectory(SongsFolder);

            Console.Out.WriteLine($"다운로드 프로세스가 시작되었습니다.");

            Console.Out.WriteLine("----------------------------------------------------");

            Parallel.ForEach(queuedBeatmapSets, new ParallelOptions() { MaxDegreeOfParallelism = 50 },ProcessBeatmapSet);

            Console.Out.WriteLine($"다운로드 완료");
            Console.Out.WriteLine($"아무키나 누르시면 종료됩니다");
            Console.In.ReadLine();
        }

        private static void ProcessBeatmapSet(IBeatmapSet beatmapSet)
        {
            try
            {
                Console.Out.WriteLineAsync($"( {downloaded} / {downloadCount} ) {beatmapSet.RankedID} {beatmapSet.RankedName} 다운로드 시작");
                string safeName = convertToSafeName(beatmapSet.RankedName);
                string fullFileName = beatmapSet.RankedID + " " + safeName + "." + beatmapSet.PackageType;
                try
                {
                    using (FileStream fileStream = File.Open(Path.Combine(SongsFolder, fullFileName),
                        FileMode.Create))
                    {
                        using (BeatmapSetFile beatmapSetFile = mirror.DowmloadBeatmap(beatmapSet, fileStream, preferNoVid))
                        {

                            Console.Out.WriteLineAsync($"( {++downloaded} / {downloadCount} ) {beatmapSet.RankedID} {beatmapSet.RankedName} 다운로드 완료");
                        }
                    }
                } catch (Exception e)
                {
                    Console.Out.WriteLine($"비트맵 파일 쓰기 오류가 발생했습니다 {fullFileName} 다운로드를 건너뜁니다 {e}");
                }
            } catch (BeatmapNotFoundException)
            {
                Console.Out.WriteLineAsync("{beatmapSet.RankedID} {beatmapSet.RankedName} 는 다운로드 불가능 상태 이므로 스킵합니다");
            } catch (Exception e)
            {
                Console.Out.WriteLineAsync($"{beatmapSet.RankedID} {beatmapSet.RankedName} 알 수 없는 오류 입니다 {e}");
            }
        }

        private static string convertToSafeName(string str)
        {
            return string.Join("_", str.Split(Path.GetInvalidFileNameChars()));
        }

        private static BeatmapDb ParseDb(string osuDirectory)
        {
            try
            {
                using (FileStream stream = File.OpenRead($"{osuDirectory}{Path.DirectorySeparatorChar}osu!.db"))
                    return OsuDbReader.ParseFromStream(stream);
            } catch (DbParseException e)
            {
                Console.Out.WriteLine($"osu!.db 읽기 실패 {e.ToString()}");
            } catch (Exception e)
            {
                Console.Out.WriteLine($"osu!.db가 존재 하지 않으므로 스킵 {e.ToString()}");
            }

            DateTime now = new DateTime();
            return new OsuDb(now.Day + now.Month * 100 + now.Year + 10000,0,false,now,"Guest",new Dictionary<int, Beatmap.IBeatmapSet>());
        }

        private static bool isValidInstallation(string osuDirectory)
        {
            return File.Exists($"{osuDirectory}{Path.DirectorySeparatorChar}osu!.exe");
        }

        private static string AskLocation(string str)
        {
            Console.Out.WriteLine(str);
            FolderBrowserDialog osuFolderSelector = new FolderBrowserDialog();
            if (osuFolderSelector.ShowDialog() == DialogResult.OK)
                return osuFolderSelector.SelectedPath;
            return null;
        }
    }
}
