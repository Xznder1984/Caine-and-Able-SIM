using System;
using System.IO;
using NAudio.Wave;

namespace CASimulator
{
    public class SoundManager
    {
        private IWavePlayer wavePlayer;
        private string soundsPath;

        public SoundManager(string basePath = null)
        {
            try
            {
                wavePlayer = new WaveOutEvent();
                soundsPath = basePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "sounds");
            }
            catch
            {
                // Audio not available
                wavePlayer = null;
            }
        }

        public bool PlaySound(string filename)
        {
            if (wavePlayer == null)
                return false;

            try
            {
                string filepath = Path.Combine(soundsPath, filename);

                if (!File.Exists(filepath))
                    return false;

                using (var audioFileReader = new AudioFileReader(filepath))
                {
                    wavePlayer.Init(audioFileReader);
                    wavePlayer.Play();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void Stop()
        {
            try
            {
                if (wavePlayer != null)
                    wavePlayer.Stop();
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                wavePlayer?.Dispose();
            }
            catch { }
        }

        // Generate simple beep sound programmatically
        public void PlayBeep(int frequency = 1000, int duration = 100)
        {
            try
            {
                // Use system beep as fallback
                System.Media.SystemSounds.Beep.Play();
            }
            catch { }
        }
    }
}
