﻿using System.ComponentModel;

using Newtonsoft.Json;

using BizHawk.Common;
using BizHawk.Emulation.Common;

namespace BizHawk.Emulation.Cores.Nintendo.Gameboy
{
	public partial class Gameboy : ISettable<Gameboy.GambatteSettings, Gameboy.GambatteSyncSettings>
	{
		public GambatteSettings GetSettings()
		{
			return _settings.Clone();
		}

		public PutSettingsDirtyBits PutSettings(GambatteSettings o)
		{
			_settings = o;
			if (IsCGBMode())
			{
				SetCGBColors(_settings.CGBColors);
			}
			else
			{
				ChangeDMGColors(_settings.GBPalette);
			}

			return PutSettingsDirtyBits.None;
		}

		public GambatteSyncSettings GetSyncSettings()
		{
			return _syncSettings.Clone();
		}

		public PutSettingsDirtyBits PutSyncSettings(GambatteSyncSettings o)
		{
			bool ret = GambatteSyncSettings.NeedsReboot(_syncSettings, o);
			_syncSettings = o;
			return ret ? PutSettingsDirtyBits.RebootCore : PutSettingsDirtyBits.None;
		}

		private GambatteSettings _settings;
		private GambatteSyncSettings _syncSettings;

		public class GambatteSettings
		{
			/* Green Palette
			private static readonly int[] DefaultPalette =
			{
				10798341, 8956165, 1922333, 337157,
				10798341, 8956165, 1922333, 337157,
				10798341, 8956165, 1922333, 337157
			};
			*/
			// Grey Scale Palette
			private static readonly int[] DefaultPalette =
			{
				0xFFFFFF, 0xAAAAAA, 0x555555, 0,
				0xFFFFFF, 0xAAAAAA, 0x555555, 0,
				0xFFFFFF, 0xAAAAAA, 0x555555, 0
			};

			public int[] GBPalette;
			public GBColors.ColorType CGBColors;

			/// <summary>
			/// true to mute all audio
			/// </summary>
			public bool Muted;

			public GambatteSettings()
			{
				GBPalette = (int[])DefaultPalette.Clone();
				CGBColors = GBColors.ColorType.gambatte;
			}


			public GambatteSettings Clone()
			{
				var ret = (GambatteSettings)MemberwiseClone();
				ret.GBPalette = (int[])GBPalette.Clone();
				return ret;
			}
		}

		public class GambatteSyncSettings
		{
			[DisplayName("Use official Nintendo BootROM")]
			[Description("Uses a provided official BootROM (or \"BIOS\") instead of built-in unofficial firmware. You must provide the BootROM. Should be used for TASing.")]
			[DefaultValue(false)]
			public bool EnableBIOS { get; set; }

			public enum ConsoleModeType
			{
				Auto,
				GB,
				GBC,
				GBA
			}

			[DisplayName("Console Mode")]
			[Description("Pick which console to run, 'Auto' chooses from ROM header; 'GB', 'GBC', and 'GBA' chooses the respective system")]
			[DefaultValue(ConsoleModeType.Auto)]
			public ConsoleModeType ConsoleMode { get; set; }

			[DisplayName("Multicart Compatibility")]
			[Description("Use special compatibility hacks for certain multicart games.  Relevant only for specific multicarts.")]
			[DefaultValue(false)]
			public bool MulticartCompat { get; set; }

			[DisplayName("Realtime RTC")]
			[Description("If true, the real time clock in MBC3 and HuC3 games will reflect real time, instead of emulated time.  Ignored (treated as false) when a movie is recording.")]
			[DefaultValue(false)]
			public bool RealTimeRTC { get; set; }

			[DisplayName("RTC Divisor Offset")]
			[Description("CPU clock frequency relative to real time clock. Base value is 2^22 Hz. Used in cycle-based RTC to sync on real hardware to account for RTC imperfections.")]
			[DefaultValue(0)]
			public int RTCDivisorOffset { get; set; }

			[DisplayName("Equal Length Frames")]
			[Description("When false, emulation frames sync to vblank.  Only useful for high level TASing.")]
			[DefaultValue(false)]
			public bool EqualLengthFrames
			{
				get => _equalLengthFrames;
				set => _equalLengthFrames = value;
			}

			[DisplayName("Display BG")]
			[Description("Display background")]
			[DefaultValue(true)]
			public bool DisplayBG { get; set; }

			[DisplayName("Display OBJ")]
			[Description("Display objects")]
			[DefaultValue(true)]
			public bool DisplayOBJ { get; set; }

			[DisplayName("Display Window")]
			[Description("Display window")]
			[DefaultValue(true)]
			public bool DisplayWindow { get; set; }

			[JsonIgnore]
			[DeepEqualsIgnore]
			private bool _equalLengthFrames;

			public GambatteSyncSettings()
			{
				SettingsUtil.SetDefaultValues(this);
			}

			public GambatteSyncSettings Clone()
			{
				return (GambatteSyncSettings)MemberwiseClone();
			}

			public static bool NeedsReboot(GambatteSyncSettings x, GambatteSyncSettings y)
			{
				return !DeepEquality.DeepEquals(x, y);
			}
		}
	}
}
