using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;

namespace LiveTunes
{
    class AppCache
    {
        public static bool Load()
        {
            // If already loaded...
            if (data != null)
                return true;

            try
            {
                IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
                if (isoFile.FileExists(Filename))
                {
                    IsolatedStorageFileStream stream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(Filename, FileMode.Open);
                    TextReader reader = new StreamReader(stream);
                    XmlSerializer serializer = new XmlSerializer(typeof(SerializedAppData));
                    data = serializer.Deserialize(reader) as SerializedAppData;
                    reader.Close();
                }
                else
                {
                    data = new SerializedAppData();
                }
                return true;
            }
            catch (FileNotFoundException)
            {
                // No file found - that's okay
                return true;
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc);
                data = new SerializedAppData();
                return false;
            }
        }

        public static void Save()
        {
            if (data == null)
                return;

            try
            {
                IsolatedStorageFileStream stream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(Filename, FileMode.Create);
                StreamWriter writer = new StreamWriter(stream);
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedAppData));
                serializer.Serialize(writer, data);
                writer.Close();
            }
            catch
            {
                // Couldn't save file - crap
            }
        }

        private static void EnsureLoaded()
        {
            Load(); // Ignore return result
        }

        public static bool CompletedFirstRun { get { EnsureLoaded(); return data.CompletedFirstRun; } set { data.CompletedFirstRun = value; } }
        public static DateTime LastAttemptedSyncTime { get { EnsureLoaded(); return data.LastAttemptedSyncTime; } set { data.LastAttemptedSyncTime = value; } }
        public static DateTime LastConcertListingSyncTime { get { EnsureLoaded(); return data.LastConcertListingSyncTime; } set { data.LastConcertListingSyncTime = value; } }
        public static string LastConcertSyncPlace { get { EnsureLoaded(); return data.LastConcertSyncPlace; } set { data.LastConcertSyncPlace = value; } }

        public static int PanoramaPageIndex { get { EnsureLoaded(); return data.PanoramaPageIndex; } set { data.PanoramaPageIndex = value; } }
        public static int CurrentConcertId { get { EnsureLoaded(); return data.CurrentConcertId; } set { data.CurrentConcertId = value; } }
        public static double ArtistListScrollPos { get { EnsureLoaded(); return data.ArtistListScrollPos; } set { data.ArtistListScrollPos = value; } }
        public static double AllConcertsListScrollPos { get { EnsureLoaded(); return data.AllConcertsListScrollPos; } set { data.AllConcertsListScrollPos = value; } }

        protected static string Filename = "AppData.xml";
        protected static SerializedAppData data;
    }

    [XmlRoot("AppData")]
    public class SerializedAppData
    {
        [XmlAttribute]
        public int Version = 1;

        public bool CompletedFirstRun;
        public DateTime LastAttemptedSyncTime;
        public DateTime LastConcertListingSyncTime;
        public string LastConcertSyncPlace;

        // State for tombstoning
        public int PanoramaPageIndex;
        public int CurrentConcertId;
        public double ArtistListScrollPos;
        public double AllConcertsListScrollPos;
    }
}
