namespace AssemblyResolverNS
{
    public class Program
    {
        private static string _aotDir;
        private static string[] _hotFiles;

        public static void Main(string[] args)
        {
            /*
             * help: 
             * -aot_dir: directory of aot which hotupdate dlls depend on
             * -hot_files: hotupdate dlls, split by ';'
             */

            var helpText =
@"
help:
-aot_dir <directory_path> : directory of aot which hotupdate dlls depend on
-hot_files <dll1_path>;<dll2_path> : hotupdate dlls, split by ';'
";
            if (args.Length == 0)
            {
                Console.WriteLine(helpText);
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-aot_dir")
                {
                    _aotDir = args[i + 1];
                }
                else if (args[i] == "-hot_files")
                {
                    _hotFiles = args[i + 1].Split(';');
                }
            }

            if (string.IsNullOrEmpty(_aotDir) || _hotFiles == null || _hotFiles.Length == 0)
            {
                Console.WriteLine(helpText);
                return;
            }

            var resolver = new Resolver(_aotDir, _hotFiles);
            resolver.Resolve();
        }
    }
}
