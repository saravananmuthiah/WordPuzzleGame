using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Plugin.Maui.Audio;

namespace WordPuzzleGame.Services
{
    // Simple cross-platform audio service for playing short sound effects from Resources/Raw
    public static class AudioService
    {
        private static IAudioPlayer? _backgroundPlayer;

        public static async Task PlaySoundAsync(string fileName)
        {
            try
            {
                var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                _backgroundPlayer = AudioManager.Current.CreatePlayer(stream);
                _backgroundPlayer.Play();
            }
            catch (Exception)
            {
                // Optionally log or ignore
            }
        }

        public static void StopSound()
        {
            if (_backgroundPlayer != null)
            {
                _backgroundPlayer.Stop();
                _backgroundPlayer.Dispose();
                _backgroundPlayer = null;
            }
        }
    }
}
