using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using RadiusNetworks.IBeaconAndroid;
//using Region = RadiusNetworks.IBeaconAndroid.Region;
using Color = Android.Graphics.Color;
using Android.Support.V4.App;
using NeuBeacons.Core;

namespace NeuBeacon.Droid
{
	[Activity(Label = "Neu Beacons", MainLauncher = true, LaunchMode = LaunchMode.SingleTask)]
	public class MainActivity : Activity, IBeaconConsumer
	{
		private const string UUID = "e4C8A4FCF68B470D959F29382AF72CE7";
		private const string regiodId = "";

		bool _paused;
		IBeaconManager _iBeaconManager;
		MonitorNotifier _monitorNotifier;
		RangeNotifier _rangeNotifier;
		Region _monitoringRegion;
		Region _rangingRegion;
		EditText _textBeaconId;
		EditText _textBeaconName;
		EditText _textBeaconDescription;
		Button _saveButton;
		bool _newBeaconDetected;
		IBeacon _iBeacon;

		int _previousProximity;


		public MainActivity()
		{
			_iBeaconManager = IBeaconManager.GetInstanceForApplication(this);

			_monitorNotifier = new MonitorNotifier();
			_rangeNotifier = new RangeNotifier();

			_monitoringRegion = new Region(regiodId, null, null, null);
			_rangingRegion = new Region(regiodId, null, null, null);
		}

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.Main);

			_textBeaconId = FindViewById<EditText>(Resource.Id.textViewBeaconId);
			_textBeaconName = FindViewById<EditText>(Resource.Id.textViewBeaconName);
			_textBeaconDescription = FindViewById<EditText>(Resource.Id.textViewBeaconDescription);

			_iBeaconManager.Bind(this);

			_monitorNotifier.EnterRegionComplete += EnteredRegion;
			_monitorNotifier.ExitRegionComplete += ExitedRegion;

			_rangeNotifier.DidRangeBeaconsInRegionComplete += RangingBeaconsInRegion;

			_saveButton = FindViewById<Button> (Resource.Id.buttonSave);

			// wire up add beacon button handler
			if(_saveButton != null) {
				_saveButton.Click += (sender, e) => {

					if(_newBeaconDetected && _iBeacon == null)
					{
						Beacon beacon = new Beacon(_newBeaconDetected);
						beacon.Name = _textBeaconName.Text;
						beacon.Notes = _textBeaconDescription.Text;
						beacon.ID = Guid.NewGuid().ToString();

						BeaconManager.SaveBeaconAsync(beacon);
					}
					else
					{
						Beacon beacon = new Beacon(_newBeaconDetected);
						beacon.Name = _textBeaconName.Text;
						beacon.Notes = _textBeaconDescription.Text;
						beacon.ID = _iBeacon.ProximityUuid;

						BeaconManager.SaveBeaconAsync(beacon);
					}
				};
			}
		}

		protected override void OnResume()
		{
			base.OnResume();
			_paused = false;
		}

		protected override void OnPause()
		{
			base.OnPause();
			_paused = true;
		}

		void EnteredRegion(object sender, MonitorEventArgs e)
		{
			if(_paused)
			{
				ShowNotification();
			}
		}

		void ExitedRegion(object sender, MonitorEventArgs e)
		{
		}

		void RangingBeaconsInRegion(object sender, RangeEventArgs e)
		{
			if (e.Beacons.Count > 0)
			{
				var newDetectedBeacon = e.Beacons.FirstOrDefault();
				if (_iBeacon != null && _iBeacon.ProximityUuid == newDetectedBeacon.ProximityUuid) {
					return;
				}

				switch((ProximityType)newDetectedBeacon.Proximity)
				{
				case ProximityType.Immediate:
					UpdateDisplay(newDetectedBeacon);
					break;
				case ProximityType.Near:
					UpdateDisplay(newDetectedBeacon);
					break;
					//case ProximityType.Far:
					//	UpdateDisplay(beacon.ProximityUuid);
					//	break;
					//case ProximityType.Unknown:
					//	UpdateDisplay(beacon.ProximityUuid);
					//	break;
				}

				_previousProximity = newDetectedBeacon.Proximity;
				_iBeacon = newDetectedBeacon;
			}
		}

		#region IBeaconConsumer impl
		public void OnIBeaconServiceConnect()
		{
			_iBeaconManager.SetMonitorNotifier(_monitorNotifier);
			_iBeaconManager.SetRangeNotifier(_rangeNotifier);

			_iBeaconManager.StartMonitoringBeaconsInRegion(_monitoringRegion);
			_iBeaconManager.StartRangingBeaconsInRegion(_rangingRegion);
		}
		#endregion

		private void UpdateDisplay(IBeacon ibeacon)
		{
			RunOnUiThread(() =>
				{
					_iBeacon = ibeacon;
					_textBeaconId.Text = ibeacon.ProximityUuid;
					var beaconResult = BeaconManager.GetBeaconAsync(ibeacon.ProximityUuid);

					if(beaconResult != null)
					{
						_textBeaconName.Text = beaconResult.Name;
							_textBeaconDescription.Text = beaconResult.Notes;
					}
					else
					{
						_newBeaconDetected = true;
					}
				});
		}
		private void ShowNotification()
		{
			var resultIntent = new Intent(this, typeof(MainActivity));
			resultIntent.AddFlags(ActivityFlags.ReorderToFront);
			var pendingIntent = PendingIntent.GetActivity(this, 0, resultIntent, PendingIntentFlags.UpdateCurrent);
			var notificationId = Resource.String.beacon_notification;

			var builder = new NotificationCompat.Builder(this)
				.SetSmallIcon(Resource.Drawable.Xamarin_Icon)
				.SetContentTitle(this.GetText(Resource.String.app_label))
				.SetContentText(this.GetText(Resource.String.beacon_notification))
				.SetContentIntent(pendingIntent)
				.SetAutoCancel(true);

			var notification = builder.Build();

			var notificationManager = (NotificationManager)GetSystemService(NotificationService);
			notificationManager.Notify(notificationId, notification);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_monitorNotifier.EnterRegionComplete -= EnteredRegion;
			_monitorNotifier.ExitRegionComplete -= ExitedRegion;

			_rangeNotifier.DidRangeBeaconsInRegionComplete -= RangingBeaconsInRegion;

			_iBeaconManager.StopMonitoringBeaconsInRegion(_monitoringRegion);
			_iBeaconManager.StopRangingBeaconsInRegion(_rangingRegion);
			_iBeaconManager.UnBind(this);
		}
	}
}