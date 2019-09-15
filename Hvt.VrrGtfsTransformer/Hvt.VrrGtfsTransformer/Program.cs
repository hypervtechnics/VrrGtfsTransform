using GTFS;
using GTFS.IO;
using GTFS.Validation;

using Hvt.VrrGtfsTransformer.Extensions;
using Hvt.VrrGtfsTransformer.Transformations;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Hvt.VrrGtfsTransformer
{
    class Program
    {
        private static Stopwatch stopwatch;
        private static bool debugOutput;
        private static List<ITransformationStep> transformationSteps;

        static void Main(string[] args)
        {
            stopwatch = new Stopwatch();
            transformationSteps = new List<ITransformationStep>()
            {
                new ParentStationTransformationStep(),
                new PlatformCodeTransformationStep()
            };

            if (args.Contains("--help") || args.Contains("--h"))
            {
                WriteHelpText();
                return;
            }

            if (args.Contains("--version") || args.Contains("--v"))
            {
                WriteVersion();
                return;
            }

            debugOutput = args.Contains("--debug") || args.Contains("--d");

            string gtfsPath;

            if (args.Length > 0 && Directory.Exists(args[args.Length - 1]))
            {
                gtfsPath = args[args.Length - 1];
            }
            else
            {
                Write("Enter the path of the GTFS feed:");
                gtfsPath = Console.ReadLine();
            }

            var outputPath = gtfsPath + "_Output";
            var output = new DirectoryInfo(outputPath);

            if (!Directory.Exists(gtfsPath))
            {
                Write("GTFS feed not found!", ConsoleColor.Red);
                Environment.ExitCode = 10;
                return;
            }

            if (!output.Exists)
            {
                Write("Creating output directory", ConsoleColor.Gray);
                output.Create();
            }

            Write("Preparing to load the GTFS feed");
            Write("Source: " + gtfsPath);
            var gtfsReader = new GTFSReader<GTFSFeed>();
            var gtfsSource = new GTFSDirectorySource(gtfsPath);

            ApplySpecialParsing(gtfsReader);

            Write("Loading GTFS feed");
            stopwatch.Start();
            var feed = gtfsReader.Read(gtfsSource);
            gtfsSource.Dispose();
            Write("Loading of GTFS feed took " + stopwatch.StopAndMeasure().TotalMilliseconds + " ms", ConsoleColor.Green);

            WriteNoPrefix("");
            PrintFeedInformation(feed);
            WriteNoPrefix("");

            //Console.ReadKey();
            Write("Starting execution of all transformations");
            var totalTimeForTransformations = TimeSpan.Zero;

            foreach (var transformer in transformationSteps)
            {
                Write("Starting transformation: " + transformer.Name);
                stopwatch.Start();

                feed = transformer.Execute(feed);

                var elapsed = stopwatch.StopAndMeasure();
                totalTimeForTransformations += elapsed;
                Write("Finished transformation (" + elapsed.TotalMilliseconds + " ms)", ConsoleColor.Green);
            }

            Write("Finished all transformations (" + transformationSteps.Count + " in " + totalTimeForTransformations.TotalMilliseconds + " ms)", ConsoleColor.Green);
            WriteNoPrefix("");
            PrintFeedInformation(feed);
            WriteNoPrefix("");

            Write("Validating gtfs feed");
            var isValidTransformedGtfs = GTFSFeedValidation.Validate(feed, out string validationMessages);

            if (isValidTransformedGtfs)
            {
                Write("Is valid", ConsoleColor.Green);
            }
            else
            {
                Write("Invalid. Reason: " + validationMessages, ConsoleColor.Red);
            }

            Write("Target: " + outputPath);
            Write("Please press any key to start the export of the transformed GTFS feed", ConsoleColor.Cyan);
            Console.ReadKey();

            Write("Preparing export of transformed GTFS feed");
            var gtfsWriter = new GTFSWriter<GTFSFeed>();
            var gtfsTarget = new GTFSDirectoryTarget(output);

            Write("Exporting the transformed GTFS feed");
            stopwatch.Start();
            gtfsWriter.Write(feed, gtfsTarget);
            Write("Exporting the transformed GTFS feed took " + stopwatch.StopAndMeasure().TotalMilliseconds + " ms", ConsoleColor.Green);

            Console.ReadKey();
        }

        private static void ApplySpecialParsing(GTFSReader<GTFSFeed> gtfsReader)
        {
            // In VRR feed all dates might be surrounded with ". E.g: "<date>"
            Write("Applying special date handling for VRR GTFS feed during parsing", ConsoleColor.Gray);
            gtfsReader.DateTimeReader = (dateString) =>
            {
                if (dateString.StartsWith("\""))
                {
                    dateString = dateString.Trim(new char[] { '"', '\'', ' ' });
                }

                return DateTime.ParseExact(dateString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            };
        }

        private static void PrintFeedInformation(GTFSFeed feed)
        {
            Write("GTFS Feed information");
            Write("Provider:   " + feed.GetFeedInfo().PublisherName);
            Write("Version:    " + feed.GetFeedInfo().Version);
            Write("Stops:      " + feed.Stops.Count);
            Write("Trips:      " + feed.Trips.Count);
            Write("Stop times: " + feed.StopTimes.Count);
            Write("Routes:     " + feed.Routes.Count);
        }

        public static void WriteVersion()
        {
            WriteNoPrefix($"Version {Assembly.GetExecutingAssembly().GetName().Version}");
        }

        public static void WriteHelpText()
        {
            WriteNoPrefix("");
            WriteNoPrefix($"GTFS Transformer for the VRR feed");
            WriteNoPrefix("\t=> Get the feed at https://openvrr.de/dataset/gtfs");
            WriteNoPrefix("");
            WriteNoPrefix("Usage:");
            WriteNoPrefix($"\tdotnet {Path.GetFileName(Assembly.GetExecutingAssembly().Location)} [--version|--v] [--debug|-d] <path>");
            WriteNoPrefix("");
            WriteNoPrefix("Parameters:");
            WriteNoPrefix("\tpath              => If the path does not exist or is not given it will ask for the path.");
            WriteNoPrefix("\t--debug | --d     => Makes some additional output in some cases.");
            WriteNoPrefix("\t--version | --v   => Outputs the version");
            WriteNoPrefix("");
            WriteNoPrefix($"Transformations ({transformationSteps.Count}):");
            transformationSteps.ForEach(s => WriteNoPrefix($"\t- {s.Name}"));
        }

        public static void WriteDebug(string text)
        {
            if (debugOutput)
            {
                Write(text, ConsoleColor.Cyan);
            }
        }

        public static void WriteWarn(string text)
        {
            Write(text, ConsoleColor.Yellow);
        }

        public static void WriteNoPrefix(string text, ConsoleColor color = ConsoleColor.White)
        {
            Write(text, color, false);
        }

        public static void Write(string text, ConsoleColor color = ConsoleColor.White, bool usePrefix = true)
        {
            string textToWrite = text;

            if (usePrefix)
            {
                var now = DateTime.Now;
                textToWrite = $"[{now.Hour.ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")}.{now.Millisecond.ToString("000")}] " + textToWrite;
            }

            Console.ForegroundColor = color;
            Console.WriteLine(textToWrite);
        }
    }
}
