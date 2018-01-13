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

        private static string DownloadFolder { get => osuLocation + Path.DirectorySeparatorChar + "Downloads"; }
        private static string SongsFolder { get => osuLocation + Path.DirectorySeparatorChar + "Songs"; }

        private static bool preferNoVid = false;

        public const string USER_AGENT = "osu!downloader";

        private static int downloading = 0;
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
            downloadCount = mirror.BeatmapSetCount - localDb.BeatmapSets.Count;

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

            if (!Directory.Exists(SongsFolder)) Directory.CreateDirectory(SongsFolder);

            Console.Out.WriteLine($"다운로드 프로세스가 시작되었습니다.");

            Parallel.ForEach(mirror.CachedList.Values, new ParallelOptions() { MaxDegreeOfParallelism = 50 },ProcessBeatmapSet);

            Console.Out.WriteLine($"다운로드 완료");
        }

        private static void ProcessBeatmapSet(OnlineBeatmapSet beatmapSet)
        {
            try
            {
                Console.Out.WriteLineAsync($"( {downloading++} / {downloadCount} ) {beatmapSet.RankedID} {beatmapSet.RankedName} 다운로드 시작");
                string safeName = convertToSafeName(beatmapSet.RankedName);
                using (FileStream fileStream = File.OpenWrite(SongsFolder + Path.DirectorySeparatorChar + beatmapSet.RankedID + " " + safeName + "." + beatmapSet.PackageType)){
                    using (BeatmapSetFile beatmapSetFile = mirror.DowmloadBeatmap(beatmapSet, fileStream, preferNoVid))
                    {

                        Console.Out.WriteLineAsync($"( {downloading} / {downloadCount} ) {beatmapSet.RankedID} {beatmapSet.RankedName} 다운로드 완료");
                    }
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
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                str = str.Replace(invalid, '_');
            }

            return str;
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
