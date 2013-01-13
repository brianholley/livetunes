using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO.IsolatedStorage;
using System.IO;

namespace LiveTunes
{
    class Settings
    {
        #region Load/Save
        public static bool Load()
        {
            // If already loaded...
            if (settings != null)
                return true;

            try
            {
                IsolatedStorageFile isoFile = IsolatedStorageFile.GetUserStoreForApplication();
                if (isoFile.FileExists(Filename))
                {
                    IsolatedStorageFileStream stream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(Filename, FileMode.Open);
                    TextReader reader = new StreamReader(stream);
                    XmlSerializer serializer = new XmlSerializer(typeof(SerializedSettings));
                    settings = serializer.Deserialize(reader) as SerializedSettings;
                    reader.Close();
                }
                else
                {
                    settings = new SerializedSettings();
                }
                return true;
            }
            catch (FileNotFoundException)
            {
                // No settings file found - that's okay
                return true;
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc);
                settings = new SerializedSettings();
                return false;
            }
        }

        public static void Save()
        {
            // If settings never loaded, they wouldn't have changed...
            if (settings == null)
                return;

            try
            {
                IsolatedStorageFileStream stream = IsolatedStorageFile.GetUserStoreForApplication().OpenFile(Filename, FileMode.Create);
                StreamWriter writer = new StreamWriter(stream);
                XmlSerializer serializer = new XmlSerializer(typeof(SerializedSettings));
                serializer.Serialize(writer, settings);
                writer.Close();
            }
            catch
            {
                // Couldn't save settings file - crap
            }
        }

        private static void EnsureLoaded()
        {
            Load(); // Ignore return result
        }
        #endregion

        private const double KilometersPerMile = 1.609344;
        public static int DefaultDistance
        {
            get { return (Units == DistanceUnits.Kilometers ? 20 : (int)Math.Ceiling(20 * KilometersPerMile)); }
        }

        public static Location Location
        {
            get
            {
                EnsureLoaded();
                if (settings.Location == null)
                    return null;

                if (!string.IsNullOrEmpty(settings.Location.Place))
                    return new Location(settings.Location.Place);
                return new Location(settings.Location.Lat, settings.Location.Long);
            }
            set { settings.Location = new SerializedLocation() { Place = value.Place, Lat = value.Lat, Long = value.Long }; }
        }
        
        public static int Distance
        {
            get 
            {
                EnsureLoaded();
                if (Units == DistanceUnits.Kilometers)
                    return settings.Distance;
                else
                    return (int)Math.Ceiling(settings.Distance * KilometersPerMile);
            } 
            set
            {
                if (Units == DistanceUnits.Kilometers)
                    settings.Distance = value;
                else
                    settings.Distance = (int)Math.Floor(value / KilometersPerMile);
            }
        }
        public static DistanceUnits Units { get { EnsureLoaded(); return settings.Units; } set { settings.Units = value; } }

        protected static string Filename = "Settings.xml";
        protected static SerializedSettings settings;
    }

    public enum DistanceUnits
    {
        Miles,
        Kilometers
    }

    [XmlRoot("Settings")]
    public class SerializedSettings
    {
        [XmlAttribute]
        public int Version = 1;

        [XmlElement]
        public SerializedLocation Location;

        public int Distance;
        public DistanceUnits Units;

        public SerializedSettings()
        {
            Distance = 20;
            Units = DistanceUnits.Miles;
        }
    }

    public class SerializedLocation
    {
        public string Place;
        public double Lat;
        public double Long;
    }
}
