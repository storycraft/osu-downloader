using Microsoft.Win32;
using OsuDownloader.Beatmap;
using OsuDownloader.DataBase;
using OsuDownloader.Exceptions.Local;
using OsuDownloader.Exceptions.Server;
using OsuDownloader.OsuDownloader.Server.Mirror.Hexide;
using OsuDownloader.Server;
using OsuDownloader.Server.Mirror.Hexide;
using System;
using System.Collections.Generic;
using System.IO;
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
            Console.Out.WriteLine("----------------------------------------------------");

            Task mirrorTask = Task.Run(() =>
            {
                int reTryCounter = 1;
                while (true)
                {
                    try
                    {
                        mirror = new OsuHexide();
                        break;
                    }
                    catch (Exception e)
                    {
                        Console.Out.WriteLine($"미러 서버 접속이 실패 했습니다 {reTryCounter} 초 뒤 재 시도 합니다 {e}");
                        System.Threading.Thread.Sleep(reTryCounter * 1000);
                        reTryCounter *= 2;
                    }
                }
            });

            try
            {
                osuLocation = TryFindOsuLocation();
            } catch(Exception)
            {

            }

            if (osuLocation == null)
            {
                Console.Out.WriteLine("오스가 설치된 폴더를 찾을 수 없습니다. 직접 위치를 지정해 주세요");
                while (true)
                {
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
                    }
                    catch (Exception)
                    {
                        Console.Out.WriteLine("잘못 된 경로 입니다");
                    }
                }
            }

            Console.Out.WriteLine($"로컬 비트맵 리스트 파싱 중...");
            localDb = ParseDb(osuLocation);
            Console.Out.WriteLine($"로컬 비트맵 리스트 파싱 완료");
            Console.Out.WriteLine($"비트맵 셋 {localDb.BeatmapSets.Count} 개가 발견 되었습니다.");

            Console.Out.WriteLine("----------------------------------------------------");

            Console.Out.WriteLine($"미러 서버에서 전체 비트맵 리스트 받아오는 중...");

            Console.Out.WriteLine("----------------------------------------------------");

            Task.WaitAny(mirrorTask);

            Console.Out.WriteLine($"미러 서버 : {mirror.MirrorSite}");
            Console.Out.WriteLine($"비트맵 셋 {mirror.BeatmapSetCount} 개가 발견되었습니다.");

            OsuHexideSearcher mirrorSearcher = new OsuHexideSearcher((OsuHexide) mirror);

            AskKeywords(mirrorSearcher);

            int counter = 1;
            List<IBeatmapSet> beatmapSets = null;
            Console.Out.WriteLine("비트맵 셋 검색중");
            while (true)
            {
                try
                {
                    beatmapSets = mirrorSearcher.GetResult();
                    break;
                } catch (Exception e)
                {
                    Console.Out.WriteLine($"미러 서버 접속이 실패 했습니다 {counter} 초 뒤 재 시도 합니다 {e}");
                    System.Threading.Thread.Sleep(counter * 1000);
                    counter *= 2;
                }
            }

            List<IBeatmapSet> downloadQueue = ExcludeLocalBeatmapSets(beatmapSets);

            downloadCount = downloadQueue.Count;

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

            Parallel.ForEach(downloadQueue, new ParallelOptions() { MaxDegreeOfParallelism = 50 },ProcessBeatmapSet);

            Console.Out.WriteLine($"다운로드 완료");
            Console.Out.WriteLine($"아무키나 누르시면 종료됩니다");
            Console.In.ReadLine();
        }

        private static string TryFindOsuLocation()
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(@"osu\shell\open\command");

            if (key == null)
                return null;

            string str = key.GetValue("").ToString();

            if (str == null)
                return null;

            string[] splited = str.Split('"');

            if (splited.Length < 2)
                return null;

            return Path.GetDirectoryName(splited[1]);
        }

        private static void AskKeywords(OsuHexideSearcher mirrorSearcher)
        {
            Console.Out.WriteLine("다운로드 받을 비트맵 셋 제목 검색 키워드를 입력해주세요 (빈 칸 일시 검색 제외)");

            string keyword = Console.In.ReadLine();
            if (!string.IsNullOrEmpty(keyword))
                mirrorSearcher.Title = keyword;

            Console.Out.WriteLine("아티스트 키워드를 입력해주세요 (빈 칸 일시 검색 제외)");

            keyword = Console.In.ReadLine();
            if (!string.IsNullOrEmpty(keyword))
                mirrorSearcher.Artist = keyword;

            Console.Out.WriteLine("태그 키워드를 입력해주세요 (빈 칸 일시 검색 제외)");

            keyword = Console.In.ReadLine();
            if (!string.IsNullOrEmpty(keyword))
                mirrorSearcher.Tags = keyword;

            Console.Out.WriteLine("비트맵 랭크 상태를 입력해주세요 (빈 칸 일시 모든 상태 선택)");
            Console.Out.WriteLine("");
            Console.Out.WriteLine("랭크 상태 목록");
            Console.Out.WriteLine("Loved = 65536, Ranked = 4096, Approved = 256 Qulified = 16, Pending = 1");
            Console.Out.WriteLine("더하기도 가능 합니다. 예) Loved + Ranked = 69632");

            keyword = Console.In.ReadLine();

            int stat;
            if (Int32.TryParse(keyword, out stat))
                mirrorSearcher.Approved = stat;
            else
                mirrorSearcher.Approved = 0x11111;
        }

        private static List<IBeatmapSet> ExcludeLocalBeatmapSets(List<IBeatmapSet> beatmapSets)
        {
            List <IBeatmapSet> copied = new List<IBeatmapSet>(beatmapSets);
            foreach (IBeatmapSet set in beatmapSets)
                if (localDb.HasBeatmapSet(set.RankedID))
                    copied.Remove(set);

            return copied;
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
