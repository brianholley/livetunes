using System;
using System.Device.Location;
using System.Net;

namespace LiveTunes
{
    public class Location
    {
        public enum LocationType
        {
            NamedPlace,
            LatLong
        }
        public LocationType Type { get; private set; }

        public bool Empty { get { return Place == default(string) && Lat == default(double) && Long == default(double); } }

        public string Place { get; private set; }
        public double Lat { get; private set; }
        public double Long { get; private set; }

        public Location(string place)
        {
            Type = LocationType.NamedPlace;
            Place = place;
        }

        public Location(double latitude, double longitude)
        {
            Type = LocationType.LatLong;
            Lat = latitude;
            Long = longitude;
        }
    }

    public delegate void LocationCallback(Location loc);
    public delegate void ErrorCallback(int error, string message);

    public class CallbackGeoCoordinateWatcher : GeoCoordinateWatcher
    {
        public CallbackGeoCoordinateWatcher(GeoPositionAccuracy accuracy, LocationCallback locationCallback, ErrorCallback errorCallback) : base(accuracy)
        {
            OnLocation= locationCallback;
            OnError = errorCallback;
        }

        public LocationCallback OnLocation { get; private set; }
        public ErrorCallback OnError { get; private set; }
    }

    public class LocationService
    {
        public static void GetLocation(LocationCallback locationCallback, ErrorCallback errorCallback)
        {
            System.Diagnostics.Debug.WriteLine("{0}: Starting location query", DateTime.Now);

            // Get location
            CallbackGeoCoordinateWatcher watcher = new CallbackGeoCoordinateWatcher(GeoPositionAccuracy.Default, locationCallback, errorCallback);
            watcher.MovementThreshold = 20; // Distance in meters - we don't care as we're stopping the watcher as soon as we get currently location
            watcher.StatusChanged += new EventHandler<GeoPositionStatusChangedEventArgs>(watcher_StatusChanged);
            watcher.PositionChanged += new EventHandler<GeoPositionChangedEventArgs<GeoCoordinate>>(watcher_PositionChanged);
            watcher.Start();
        }

        private static void watcher_StatusChanged(object sender, GeoPositionStatusChangedEventArgs e)
        {
            CallbackGeoCoordinateWatcher watcher = sender as CallbackGeoCoordinateWatcher;
            switch (e.Status)
            {
                case GeoPositionStatus.Disabled:
                    string message = (watcher.Permission == GeoPositionPermission.Denied ? "livetunes is not allowed to access location on this device" : "location service is currently offline");
                    watcher.Stop();
                    Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
                    {
                        watcher.OnError((int)e.Status, message);
                    });
                    break;

                case GeoPositionStatus.NoData:
                    watcher.Stop();
                    Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
                    {
                        watcher.OnError((int)e.Status, "location data is not available at the moment");
                    });
                    break;

                case GeoPositionStatus.Initializing:
                    break;

                case GeoPositionStatus.Ready:
                    break;
            }
        }

        private static void watcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            System.Diagnostics.Debug.WriteLine("{0}: Got location", DateTime.Now);

            CallbackGeoCoordinateWatcher watcher = sender as CallbackGeoCoordinateWatcher;
            Location loc = new Location(e.Position.Location.Latitude, e.Position.Location.Longitude);
            watcher.Stop();

            Microsoft.Phone.Reactive.Scheduler.Dispatcher.Dispatcher.BeginInvoke(() =>
            {
                watcher.OnLocation(loc);
            });

            Settings.Location = loc;
        }

        public static void ConvertPlaceToLatLong()
        {
        }
    }
}
