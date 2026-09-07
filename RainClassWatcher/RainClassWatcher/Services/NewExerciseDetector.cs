using RainClassWatcher.Models;
using System.Collections.Generic;
using System.Linq;

namespace RainClassWatcher.Services
{
    public sealed class NewExerciseDetector
    {
        private const int FingerprintDepth = 3;
        private const int MaxHistory = 100;

        private readonly HashSet<string> _notifiedFingerprints = [];
        private readonly Queue<string> _history = [];

        public bool ShouldNotify(RainClassScanResult result)
        {
            if (!result.HasNewExercise || result.Latest is null)
            {
                return false;
            }

            string fingerprint = BuildFingerprint(result);

            // 已经针对这个题目列表状态提醒过
            if (!_notifiedFingerprints.Add(fingerprint))
            {
                return false;
            }

            _history.Enqueue(fingerprint);

            // 防止程序长期运行后集合无限增长
            while (_history.Count > MaxHistory)
            {
                string oldest = _history.Dequeue();
                _notifiedFingerprints.Remove(oldest);
            }

            return true;
        }

        private static string BuildFingerprint(
            RainClassScanResult result)
        {
            return string.Join(
                "|",
                result.Exercises
                    .Take(FingerprintDepth)
                    .Select(exercise => exercise.Page));
        }
    }
}