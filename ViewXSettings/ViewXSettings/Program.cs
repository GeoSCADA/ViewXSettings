// ViewXSettings program
// Read and write settings files
// Steve Beadle, stephen.beadle@se.com
// Last modified 28 July 2021 - Added extended logic flag for V84
// Developed for V83, may be compatible with some earlier versions,
// If the format changes, then you will get '*** Error' messages and 
// you will need to consult or be advised of changes to the DAT files
// in newer or older versions of Geo SCADA.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ViewXSettings
{
	class Program
	{
		static BinaryReader binReader;
		static BinaryWriter binWriter;
		static string Errors = "";
		static string changeKey = "";
		static string newValue = "";
		static bool changed = false;
		static string filetype = "g";

		static int Main(string[] args)
		{
			Console.WriteLine("ViewX Settings Reader and Changer");
			Console.WriteLine("Arguments: [username] [type] [key-to-change] [new-value]");
			Console.WriteLine("[username] will find the correct folder in AppData\\Roaming, ");
			Console.WriteLine("    e.g. \"\" is default user, or use \"VVXLocalUser\"");
			Console.WriteLine("[type] is g(lobal) or the head number (1,2,...) e.g. \"g\"");
			Console.WriteLine("[key-to-change] is optional - run first without this to see all keys and values.");
			Console.WriteLine("[new-value] is needed if you supply a key.");
			Console.WriteLine("e.g. \"VVXLocalUser\" g GuestGlobalInactivityTimeout 1");
			Console.WriteLine("e.g. \"VVXLocalUser\" g GeneralcheckViewXLogoutWithNoDirtyDocuments 15");
			Console.WriteLine("e.g. \"\" 1 GuestProfileFullScreenState 1");

			string AppDataLocation = "";
			string filePath = "\\Schneider Electric\\ClearSCADA\\ViewX Global Settings.dat";

			if (args.Length == 0)
			{
				// No arguments - default to current user and global, no change of settings
				AppDataLocation = Environment.GetEnvironmentVariable("AppData");
			}
			if (args.Length >= 1)
			{
				// One or more arguments - the username is blank?
				if (args[0] == "")
				{
					AppDataLocation = Environment.GetEnvironmentVariable("AppData");
				}
				else
				{
					AppDataLocation = "C:\\Users\\" + args[0] + "\\AppData\\Roaming";
				}
			}
			if (args.Length >= 2)
			{
				// Two or more arguments - the type is blank?
				if (args[1] != "")
				{
					filetype = args[1].ToLower();
				}
			}
			if (args.Length == 4)
			{
				// Substitute a key's value
				changeKey = args[2];
				newValue = args[3];
				Console.WriteLine("Replace value of: " + changeKey + " with: " + newValue);
			}
			else
			{
				if (args.Length == 3 || args.Length > 4)
				{
					Console.WriteLine("Incorrect arguments");
				} 
			}

			if (filetype != "g")
			{
				filePath = "\\Schneider Electric\\ClearSCADA\\ViewX Head " + filetype + " Settings.dat";
			}

			string fileName = AppDataLocation + filePath;
			string newFileName = AppDataLocation + filePath + "_new";

			Console.WriteLine("");

			Console.WriteLine("Read from: " + fileName);

			if (filetype == "g")
			{
				ReadWriteSettings(fileName, newFileName);
			} else
			{
				ReadWriteHeadSettings(fileName, newFileName);
			}

			if ( Errors != "")
			{
				Console.WriteLine("*** ERRORS ***\n" + Errors + "*** FILE INVALID ***" );
#if DEBUG
				Console.ReadKey();
#endif
				return 1;
			}
			else
			{
				if (changed)
				{
					// Delete old
					try
					{
						File.Delete(fileName + "_old");
					}
					catch 
					{
						// Ignore delete errors
					}
					// Rename current old
					File.Move(fileName, fileName + "_old");
					// Rename new to current
					File.Move(newFileName, fileName);

					Console.WriteLine("Replaced value of: " + changeKey + " with: " + newValue);

					Console.WriteLine("Replaced settings file, backup is:  " + fileName + "_old");
				}
				else
				{
					if (changeKey != "")
					{
						Console.WriteLine("Did not find setting: " + changeKey);
					}
				}
#if DEBUG
				Console.ReadKey();
#endif
				return 0;
			}
		}

		// **************** SETTING READ AND CHANGE **********************************************************************
		private static void ReadWriteSettings(string fileName, string newFileName)
		{
			try
			{
				binReader = new BinaryReader(File.Open(fileName, FileMode.Open));
				binWriter = new BinaryWriter(File.Open(newFileName, FileMode.Create));
			}
			catch( Exception e)
			{
				Errors = e.Message;
				return;
			}

			// void CSettingsManager::LoadGlobalSettings()

			ReadByte("FormatName1", Encoding.ASCII.GetBytes("S")[0]);
			ReadByte("FormatName2", Encoding.ASCII.GetBytes("C")[0]);
			ReadByte("FormatName3", Encoding.ASCII.GetBytes("X")[0]);

			// Was ReadUInt32("Version", 29); V30 adds tooltip delay
			UInt32 MainVersion = ReadUInt32("Version");
			if (MainVersion < 29)
			{
				Console.WriteLine("*** Error ***  MainVersion out of range: " + MainVersion.ToString());
				Errors += "MainVersion error, Value is " + MainVersion.ToString() + ", not 29...30.\n";
			}

			// void CViewXAppBase::LoadGuestSettings(CArchive & Ar, DWORD Version)
			// void CUserProfile::LoadGlobalSettings( CArchive &Ar )

			ReadUInt16("GuestGlobalVersion", 20);

			UInt32 GuestGlobalFeatureLen = ReadUInt32("GuestGlobalFeatureLen");
			if (GuestGlobalFeatureLen < 39 || GuestGlobalFeatureLen > 40)
			{
				Console.WriteLine("*** Error ***  GuestGlobalFeatureLen out of range: " + GuestGlobalFeatureLen.ToString());
				Errors += "GuestGlobalFeatureLen error, Value is " + GuestGlobalFeatureLen.ToString() + ", not 39...40.\n";
			}
			ReadChar("MDI");
			ReadChar("ShowHomePageAtLogon");
			ReadChar("DoubleClickEditsDocuments");
			ReadChar("DoubleClickShowsProperties");
			ReadChar("FullScreen");
			ReadChar("CloseViewX");
			ReadChar("Operate");
			ReadChar("Alarms");
			ReadChar("Events");
			ReadChar("ViewConfig");
			ReadChar("ConfigureDocuments");
			ReadChar("ConfigureDatabase");
			ReadChar("ConfigureOptions");
			ReadChar("ConfigureFiles");
			ReadChar("AllowBellDisable");
			ReadChar("DisableBellByDefault");
			ReadChar("MultipleAlarmSelect");
			ReadChar("AlarmBannerAtTop");
			ReadChar("ResizeAlarmBanner");
			ReadChar("MoveAlarmBanner");
			ReadChar("RemoveAlarmBanner");
			ReadChar("OperateBar");
			ReadChar("FileBar");
			ReadChar("NavigateBar");
			ReadChar("MimicBar");
			ReadChar("AlignmentBar");
			ReadChar("ActiveXBar");
			ReadChar("SCXDataBar");
			ReadChar("SCXQueriesBar");
			ReadChar("OPCDataBar");
			ReadChar("OPCHistoricBar");
			ReadChar("Favourites");
			ReadChar("SCXDocStoreBar");
			ReadChar("DebugBar");
			ReadChar("SCXAreaBar");
			ReadChar("CanFilterBanner");
			ReadChar("UseServerLabel");
			ReadChar("AlarmSummary");
			if (GuestGlobalFeatureLen >= 40)
			{
				ReadChar("ConfigureExtendedLogic");
			}

			ReadByte("GuestGlobalFeatureType");

			ReadUInt32("GuestGlobalSelectedLocale");

			ReadUString("GuestGlobalDateFormat");

			ReadUString("GuestGlobalNumberFormat");

			ReadByte("GuestGlobalTimesInUTC");

			ReadUInt32("GuestGlobalInactivityTimeout");

			ReadByte("GuestGlobalMinAlarmBannerRows");

			ReadUString("GuestGlobalHomePage");

			ReadUString("GuestGlobalHomePageClass");

			ReadByte("GuestGlobalConfirmMethods");

			ReadUInt32("GuestGlobalBannerFilterTimeout");

			// Runs twice - for GuestProfile and thenSuperProfile
			string pre = "Guest";
			for (int p = 1; p <= 2; p++)
			{
				// void CUserProfile::LoadGlobalProfile(CArchive & Ar)
				ReadUInt16(pre + "UsertGlobalVersion", 5);

				// void CFavouriteNode::Serialize(CArchive & Ar)
				ReadUInt16(pre + "UserFavouritesVersion", 3);

				//void CFavouriteNode::Serialize(CArchive & Ar, WORD RootVersion)
				//name is blank
				ReadUString(pre + "UserGlobalFavouritesName");

				//, type zero==0
				ReadByte(pre + "UserGlobalFavouritesType", 0);

				// Should be zero, no children
				ReadUInt16(pre + "UserGlobalFavouritesChildrenSize", 0);

				// Back into void CUserProfile::LoadGlobalProfile( CArchive &Ar )

				ReadByte(pre + "UserGlobalMimicPicksMode");

				// Load WPF settings (not used by native code, but needed for guest profile serialization)
				UInt32 GuestUserGlobalWPFSettingsCount = ReadUInt32(pre + "UserGlobalWPFSettingsCount");

				for (int i = 1; i <= GuestUserGlobalWPFSettingsCount; i++)
				{
					ReadUString(pre + "UserGlobalWPFSettingsCountSetting" + i.ToString());

					byte t = ReadByte(pre + "UserGlobalWPFSettingsType" + i.ToString());

					switch (t)
					{
						// Type changes the data size!
						case 3:
							{
								ReadByte(pre + "UserGlobalWPFSettingsValue(byte)" + i.ToString());
							}
							break;
						case 9:
							{
								ReadUInt32(pre + "UserGlobalWPFSettingsValue(UInt32)" + i.ToString());
							}
							break;
						case 18:
							{
								ReadUString(pre + "UserGlobalWPFSettingsValue(CString)" + i.ToString());
							}
							break;
						default:
							Console.WriteLine("*** Error, not recognised " + pre + "UserGlobalWPFSettingsType.");
							Console.ReadKey();
							return;

					}
				}

				// One more time around
				pre = "Super";
			}

			// Back out
			// Now in: 		CAlarmBar::SerializeGlobal( Ar );

			ReadUInt16("AlarmBarVersion", 15);

			// Bar font
			ReadUString("AlarmBarFontFamily");
			ReadUInt32("AlarmBarFontHeight");
			ReadUInt32("AlarmBarFontStyle");

			// Bell font
			ReadUString("AlarmBarBellFontFamily");
			ReadUInt32("AlarmBarBellFontHeight");
			ReadUInt32("AlarmBarBellFontStyle");

			// Misc
			ReadByte("AlarmBarDefaultSortOrder");
			ReadUInt32("AlarmBarFlashColumn");
			ReadByte("AlarmBarAllowColumnResize");

			ReadUString("AlarmBarDefaultAlarmSoundPath");

			ReadUInt32("AlarmBarUseBackColour"); // Bool
			ReadUInt32("AlarmBarBackColour");

			ReadByte("AlarmBarAllowSorting");
			ReadUInt32("AlarmBarActiveHead");
			ReadByte("AlarmBarIncludeSimpleEvents");
			ReadByte("AlarmBarAutoScrollToTop");

			ReadByte("AlarmBarArrowEnabled");

			ReadByte("AlarmBarKeepHighlightedAlarmVisible");

			ReadUString("AlarmBarFilteredSymbol");
			ReadUString("AlarmBarModifiedSymbol");


			// CColourMenu::SerializeGlobal( Ar );
			for (int I = 0; I < 16; ++I)
			{
				ReadUInt32("ColourMenu" + I.ToString());
			}

			// CGraphDoc::SerializeGlobal( Ar );
			ReadUInt16("GraphDocVersion", 15);

			// Title
			ReadUString("GraphDocTitle");
			ReadUInt32("GraphDocTitleColour");

			// Axes
			pre = "GraphDocXAxis";
			// void _Defaults::_XAxis::Serialize( CArchive &Ar, WORD Version )
			//XAxis.Serialize(Ar);

			// Range
			ReadUInt32(pre + "Mode");
			ReadByte(pre + "Continuous");
			ReadByte(pre + "Jump");
			ReadUString(pre + "Offset");
			ReadUString(pre + "Interval");

			// Appearance
			ReadUInt32(pre + "Colour");
			ReadUInt32(pre + "Pos");
			ReadUInt32(pre + "Format");

			// Ticks
			ReadUInt32(pre + "MinorTicks");

			// Misc
			ReadByte(pre + "ShowGrid");
			ReadByte(pre + "ShowNow");

			// void _Defaults::_XAxis::Serialize( CArchive &Ar, WORD Version )
			//YAxis.Serialize(Ar);
			pre = "GraphDocYAxis";

			// Scale
			ReadUInt32(pre + "Mode");
			ReadDouble(pre + "Minimum");
			ReadDouble(pre + "Maximum");
			ReadByte(pre + "Logarithmic");
			ReadByte(pre + "Inverted");

			// Appearance
			ReadUInt32(pre + "Pos");
			ReadUInt32(pre + "Colour");
			ReadUString(pre + "Format");

			// Label
			ReadUString(pre + "Label");
			ReadUInt32(pre + "LabelPos");
			ReadUInt32(pre + "LabelStyle");

			// Ticks
			ReadByte(pre + "AutoTicks");
			ReadUInt32(pre + "MajorTicks");
			ReadUInt32(pre + "MinorTicks");

			// Misc
			ReadByte(pre + "ShowGridDefault");
			ReadByte(pre + "ShowGridAll");


			// Traces
			//Traces.Serialize(Ar);
			pre = "GraphDocTraces";
			ReadByte(pre + "ShowLimits");
			ReadByte(pre + "ShowMarkers");
			ReadByte(pre + "ShowAnnotations");
			ReadByte(pre + "ExtendToNow");

			ReadByte(pre + "MaxMarker");
			for (int I = 0; I < 6; ++I)
			{
				ReadUInt32(pre + "MarkerColours" + I.ToString());
			}

			// Fonts
			string[] prearr = { "Main", "Title", "Label", "Key" };
			foreach (string p in prearr)
			{
				pre = "GraphDocFonts" + p;
				ReadUString(pre + "Family");
				ReadDouble(pre + "Height");
				ReadUInt32(pre + "Style");
			}
			pre = "GraphDoc";
			// Key
			ReadByte(pre + "ShowKey");

			int GraphDocKeyColumnsSize = ReadByte(pre + "KeyColumnsSize");
			for (int I = 0; I < GraphDocKeyColumnsSize; ++I)
			{
				ReadUInt32(pre + "KeyColumns" + I.ToString());
			}

			// Misc
			ReadUInt32(pre + "BackColour");
			ReadUInt32(pre + "LimitLabelPos");
			ReadByte(pre + "AutoShowRulerDlg");

			//  Specific Defaults
			//	CurrentTraceDefaults.Serialize(Ar);
			//	RawHistoricTraceDefaults.Serialize(Ar);
			//	ProcessedHistoricTraceDefaults.Serialize(Ar);
			string[] prearrtd = { "CurrentTraceDefaults", "RawHistoricTraceDefaults", "ProcessedHistoricTraceDefaults" };
			foreach (string p in prearrtd)
			{
				pre = p;
				ReadUInt16(pre + "Version", 1);

				// Traces
				ReadByte(pre + "TraceStyle");
				ReadUInt64(pre + "BarWidth");
				ReadUInt64(pre + "GapInterval");
				ReadByte(pre + "UpdateMode");
				ReadUInt64(pre + "Interval");
				ReadByte(pre + "RawOverlay");
				ReadUInt32(pre + "PointLimit");

				// Lines
				ReadByte(pre + "LineStyle");
				ReadDouble(pre + "LineWidth");

				// Markers
				ReadUInt32(pre + "MarkerLimit");
				ReadByte(pre + "MarkerSize");
				ReadByte(pre + "MarkerStyle");

				// Annotations
				ReadByte(pre + "AnnotationSize");
				ReadByte(pre + "AnnotationStyle");
			}


			// Trace and Axes Fading
			ReadByte(pre + "EnableGradualFade");
			ReadUInt32(pre + "FadeDuration");

			// void CDrwGroup::_Defaults::Serialize( CArchive &Ar )
			// void CDrwFontInfo::Serialize( CArchive &Ar, const CArchiveVersion &ServerVersion )
			ReadUInt16("MimicDrwGroupFontVersion", 8);

			// Mimic Defaults
			//TxtFont.Serialize(Ar, ServerVersion);
			CDrwFontInfo("MimicTxtFont");

			//TxtPen.Serialize(Ar, ServerVersion);
			CDrwBrushInfo("MimicTxtPen");
			//PlyFill.Serialize(Ar, ServerVersion);
			CDrwBrushInfo("MimicPlyFill");

			//PlyLine.Serialize(Ar, ServerVersion);
			CDrwPenInfo("MimicPlyLine");

			//BtnFont.Serialize(Ar, ServerVersion);
			CDrwFontInfo("MimicBtnFont");

			//TxtFill.Serialize(Ar, ServerVersion);
			CDrwBrushInfo("MimicTxtFill");

			//BtnFill.Serialize(Ar, ServerVersion);
			CDrwBrushInfo("MimicBtnFill");

			//BtnPen.Serialize(Ar, ServerVersion);
			CDrwBrushInfo("MimicBtnPen");

			//PipeFill.Serialize(Ar, ServerVersion);
			// void CDrwPipeBrush::Serialize( CArchive &Ar, const CArchiveVersion &ServerVersion )
			pre = "MimicPipeBrush";
			ReadByte(pre + "Version", 1);

			ReadByte(pre + "Blink");
			byte FlashModeAndOffset = ReadByte(pre + "FlashModeAndOffset");
			int FlashMode = FlashModeAndOffset >> 4;

			ReadUInt32(pre + "FillColours0");
			ReadUInt32(pre + "GradColours0");

			if (FlashMode == 4 || FlashMode == 5 || FlashMode == 6)
			{
				ReadUInt32(pre + "FillColours3");
				ReadUInt32(pre + "GradColours3");
			}

			pre = "Mimic";
			ReadUInt32(pre + "BackColour");
			ReadUInt32(pre + "ImageAlign");
			ReadDouble(pre + "PipeWidth");

			ReadUInt32(pre + "RulerEnabled"); //BOOL

			ReadUInt32(pre + "GridEnabled"); //BOOL
			ReadUInt32(pre + "GridVisible"); //BOOL

			ReadUInt32(pre + "GridSizeX"); //CSize
			ReadUInt32(pre + "GridSizeY");
			ReadUInt32(pre + "GridSpacingX");
			ReadUInt32(pre + "GridSpacingY");

			ReadByte(pre + "ZoomEnable");
			ReadUInt32(pre + "MinZoom");
			ReadUInt32(pre + "MaxZoom");

			ReadByte(pre + "AntiAliasEnabled"); //bool

			//TxtBorder.Serialize(Ar, ServerVersion);
			CDrwPenInfo("MimicTxtBorder");

			// CXYZPlotDoc::SerializeGlobal( Ar );
			ReadUInt16("XYZPlotVersion", 4);
			//Plot.Serialize(Ar);
			pre = "XYZPlot";

			ReadByte(pre + "DrawMode");
			ReadUInt32(pre + "BackColour");

			ReadUString(pre + "PlotTitle");
			ReadUInt32(pre + "TitleColour");

			//TitleFont.Serialize(Ar);
			CXYZFontInfo(pre + "TitleFont");

			// 'Global' line/marker styles
			ReadByte(pre + "GlobalLineStyle");
			ReadDouble(pre + "GlobalLineWidth");
			ReadUInt32(pre + "GlobalLineColour");

			ReadByte(pre + "GlobalMarkerSize");
			ReadByte(pre + "GlobalMarkerStyle");


			//XAxis.Serialize(Ar);
			pre = "XYZXAxis";
			CXYZFontInfo(pre + "LabelFont");
			CXYZFontInfo(pre + "TickFont");

			ReadUInt32(pre + "Colour");

			ReadUString(pre + "Label");

			ReadUInt32(pre + "LabelColour");

			// X-Axis specific
			ReadByte(pre + "AxisPos");
			ReadByte(pre + "LabelPos");

			ReadDouble(pre + "AxisDefaultRangefirst");
			ReadDouble(pre + "AxisDefaultRangesecond");

			ReadUString(pre + "AxisRangeUnits");

			ReadByte(pre + "AxisRangeLabels"); //bool

			//YAxis.Serialize(Ar);
			pre = "XYZYAxis";
			CXYZFontInfo(pre + "LabelFont");
			CXYZFontInfo(pre + "TickFont");

			ReadUInt32(pre + "Colour");

			ReadUString(pre + "Label");

			ReadUInt32(pre + "LabelColour");

			// Y-axis specific
			ReadByte(pre + "AxisPos");
			ReadByte(pre + "LabelPos");

			ReadUString(pre + "Format");

			ReadByte(pre + "ScaleType");

			ReadByte(pre + "Inverted"); //bool
			ReadDouble(pre + "Minimum");
			ReadDouble(pre + "Maximum");

			ReadByte(pre + "AutoTicks");
			ReadUInt32(pre + "NumMinorTicks");
			ReadUInt32(pre + "NumMajorTicks");


			//TimeAxis.Serialize(Ar);
			pre = "XYZTimeAxis";
			// Range
			ReadByte(pre + "Range.Mode");
			ReadUString(pre + "Range.Offset");
			ReadUString(pre + "Range.Interval");
			ReadByte(pre + "Range.Continuous");

			ReadUString(pre + "ResampleOffset");
			ReadUString(pre + "ResampleInterval");

			CXYZFontInfo(pre + "TickFont");

			ReadByte(pre + "Format");

			ReadUInt32(pre + "Colour");
			ReadByte(pre + "Inverted"); //bool

			ReadByte(pre + "AxisPos");

			ReadUInt32(pre + "AxisLen");
			ReadUInt32(pre + "AxisAngle");



			//Trace.Serialize(Ar);
			pre = "XYZTrace";
			ReadByte(pre + "LineStyle");
			ReadDouble(pre + "LineWidth");
			ReadByte(pre + "MarkerSize");
			ReadByte(pre + "MarkerStyle");
			ReadUInt32(pre + "SliceAlpha");

			int MaxMarker = ReadByte(pre + "MaxMarker");

			for (int Idx = 0; Idx < MaxMarker; ++Idx)
			{
				ReadUInt32(pre + "MarkerColours" + Idx.ToString());
			}


			// General Settings
			pre = "General";
			ReadUInt32(pre + "maxWindows");

			pre = "List";
			// CListDoc::SerializeGlobal( Ar );
			ReadUInt16(pre + "Version", 3);

			// Font
			ReadUString("FontFamily");
			ReadUInt32(pre + "FontHeight");
			ReadUInt32(pre + "FontStyle");

			// Header font
			ReadUString("HeadFontFamily");
			ReadUInt32(pre + "HeadFontHeight");
			ReadUInt32(pre + "HeadFontStyle");

			// Header colours
			ReadUInt32(pre + "HeadTextColour");
			ReadUInt32(pre + "HeadFaceColour");
			ReadUInt32(pre + "HeadDkShadowColour");
			ReadUInt32(pre + "HeadHiliteColour");
			ReadUInt32(pre + "HeadShadowColour");

			// Grid lines
			ReadUInt32(pre + "GridSize");
			ReadUInt32(pre + "GridColour");

			ReadUInt32(pre + "ListBackgroundColour");
			ReadByte(pre + "IgnoreObjectColour");


			// CDataSource::SerializeGlobal( Ar );
			pre = "DataSource";
			ReadUInt16(pre + "Version", 1);

			int DataSourceSize = ReadUInt16("DataSourceSize");    // 2

			for (int I = 0; I < DataSourceSize; ++I)
			{
				ReadByte(pre + "PickMenu" + I.ToString());        // f4 or e8
																  //(*I)->Serialize(Ar, ServerVersion);
				ReadUInt16(pre + "PickMenuType" + I.ToString());  // Guessed, 1 or 3
				ReadByte(pre + "PickMenuCode" + I.ToString(), 0); // Guessed, 0

			}

			pre = "General";

			ReadByte(pre + "startFullScreen");

			ReadByte(pre + "EnableAntiAlias");

			ReadByte(pre + "PrintViaBitmap");

			if (MainVersion >= 30)
			{
				ReadUInt32(pre + "tooltipDelay");
			}

			ReadUString(pre + "clockFormat");

			ReadByte(pre + "clockInUTC");

			ReadUString(pre + "InfoDlgFontFamily");
			ReadUInt32(pre + "InfoDlgFontHeight");

			ReadUInt32(pre + "FlashInterval");

			ReadByte(pre + "KeypadEnabled");

			ReadByte(pre + "useRemoteHelp");

			ReadByte(pre + "checkCanCloseViewX");

			ReadByte(pre + "checkViewXLogoutWithNoDirtyDocuments");

			ReadUInt32(pre + "CacheSize");

			binReader.Close();
			binWriter.Close();
		}

		// **************** COMMON SETTING GROUPS **********************************************************************
		private static void CXYZFontInfo(string pre)
		{
			// Write font information
			ReadUString(pre + "Family");
			ReadDouble(pre + "Height");
			ReadUInt32(pre + "Style");
		}

		private static void CDrwFontInfo(string pre)
		{
			//void CDrwFontInfo::Serialize(CArchive & Ar, const CArchiveVersion &ServerVersion )
			// Write the version.
			ReadByte(pre + "Version", 5);

			// Write font information
			ReadUString(pre + "Family");
			ReadFloat(pre + "Height");
			ReadUInt32(pre + "Style");

			//BYTE Alignment = BYTE(m_HAlign) | (BYTE(m_VAlign) << 4);
			ReadByte(pre + "Alignment");
			ReadByte(pre + "Orientation");
		}

		//
		private static void CDrwPenInfo(string pre)
		{
			ReadByte(pre + "Version", 4);

			ReadByte(pre + "Type");

			ReadByte(pre + "Blink");

			//BYTE FlashModeAndOffset = (BYTE)(((DWORD)m_FlashMode << 4) | m_FlashOffset);
			byte FlashModeAndOffset = ReadByte(pre + "FlashModeAndOffset");
			int FlashMode = FlashModeAndOffset >> 4;
			ReadUInt32(pre + "Colours0");
			if ((FlashMode == 4) ||
				(FlashMode == 5) ||
				(FlashMode == 6))
			{
				ReadUInt32(pre + "Colours3");
			}

			ReadFloat(pre + "Width");

			ReadByte(pre + "DashStyle");
			ReadByte(pre + "LineStyle");
			ReadByte(pre + "DashCap");

			ReadByte(pre + "JoinStyle");
			ReadFloat(pre + "MitreLimit");

			int StartCap = ReadByte(pre + "StartCap");
			if (StartCap != 0)
				ReadUInt32(pre + "StartCapSize");
			int EndCap = ReadByte(pre + "EndCap");
			if (EndCap != 0)
				ReadUInt32(pre + "EndCapSize");

		}

		//void CDrwBrushInfo::Serialize( CArchive &Ar, const CArchiveVersion &ServerVersion )
		private static void CDrwBrushInfo(string pre)
		{
			ReadByte(pre + "Version", 3);

			int Type = ReadByte(pre + "Type");

			ReadByte(pre + "Blink");

			//BYTE FlashModeAndOffset = (BYTE)(((DWORD)m_FlashMode << 4) | m_FlashOffset);
			byte FlashModeAndOffset = ReadByte(pre + "FlashModeAndOffset");
			int FlashMode = FlashModeAndOffset >> 4;
			ReadUInt32(pre + "FillColours0");
			ReadUInt32(pre + "GradColours0");

			if ((FlashMode == 4) ||
				(FlashMode == 5) ||
				(FlashMode == 6))
			{
				ReadUInt32(pre + "FillColours3");
				ReadUInt32(pre + "GradColours3");
			}

			ReadByte(pre + "HatchStyle");

			if ((Type == 4) ||
				(Type == 5) ||
				(Type == 6))
			{
				ReadUInt32(pre + "GradAngle");
				ReadUInt32(pre + "GradXPos");
				ReadUInt32(pre + "GradYPos");
			}
		}

		// **************** HEAD SPECIFIC **********************************************************************
		private static void ReadWriteHeadSettings(string fileName, string newFileName)
		{
			try
			{
				binReader = new BinaryReader(File.Open(fileName, FileMode.Open));
				binWriter = new BinaryWriter(File.Open(newFileName, FileMode.Create));
			}
			catch (Exception e)
			{
				Errors = e.Message;
				return;
			}


			ReadByte("FormatName1", Encoding.ASCII.GetBytes("S")[0]);
			ReadByte("FormatName2", Encoding.ASCII.GetBytes("C")[0]);
			ReadByte("FormatName3", Encoding.ASCII.GetBytes("X")[0]);

			ReadUInt32("Version", 1);

			// SettingsManager.cpp line 399
			// void CSettingsManager::SaveHeadSpecificSettings( DWORD Head ) const
			// GetAppBase()->SaveHeadSpecificProfileSettings(Ar);
			// m_pGuestProfile->SaveHeadSpecific(Ar);
			SaveHeadSpecific("GuestProfile");
			SaveHeadSpecific("SuperProfile");

			// CAlarmBar::SerializeHeadSpecific( Ar );
			ReadUInt16("CAlarmBar" + "Version", 7);
			// Store column order and widths
			UInt32 AlarmColCount = ReadUInt32("CAlarmBarColCount");
			for (int i = 0; i < AlarmColCount; i++)
			{
				ReadUInt32("CAlarmBarCol" + i.ToString() + "Index");
				ReadUString("CAlarmBarCol" + i.ToString() + "Name");
				ReadUInt32("CAlarmBarCol" + i.ToString() + "Width");
			}

			// CAlarmDoc::SerializeHeadSpecific( Ar );
			ReadUInt16("CAlarmDoc" + "Version", 3);
			// Store column order and widths
			UInt32 AlarmDocCount = ReadUInt32("CAlarmDocColCount");
			for (int i = 0; i < AlarmDocCount; i++)
			{
				ReadUInt32("CAlarmDocCol" + i.ToString() + "Index");
				ReadUString("CAlarmDocCol" + i.ToString() + "Name");
				ReadUInt32("CAlarmDocCol" + i.ToString() + "Width");
			}

			binReader.Close();
			binWriter.Close();
		}

		//void CUserProfile::SaveHeadSpecific( CArchive &Ar ) const
		private static void SaveHeadSpecific(string pre)
		{
			ReadUInt16(pre + "Version", 5);
			// Following 84 bytes for this function, 86 total
			// m_pNormalState->Serialize( Ar );
			byte[] NormalStateData = ReadBytes(14, pre + "NormalStateData");
			//02 00 00 00 80 07 00 00 b0 04 00 00 00 00

			//00 
			byte FullScreenState = ReadByte(pre + "FullScreenState");
			if (FullScreenState != 0)
			{
				//m_pFullScreenState->Serialize( Ar );
				byte[] FullScreenData = ReadBytes(14, pre + "FullScreenData");
			}

			//Ar << lengthof( m_NavigatorSizes );
			//07 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 
			UInt32 NavigatorSizes = ReadUInt32(pre + "NavigatorSizes");
			for (int i = 0; i < NavigatorSizes; i++)
			{
				//Ar << m_NavigatorSizes[I];
				//7 * 4 * 2 = 56 bytes
				ReadUInt32(pre + "NavigatorSizeX" + i.ToString());
				ReadUInt32(pre + "NavigatorSizeY" + i.ToString());
			}

			//Ar << (BYTE)m_NavigatorWindow;
			//01
			ReadByte(pre + "NavigatorWindow");

			//Ar << m_AlarmBarSize;
			//00 00 00 00 00 00 00 00
			ReadUInt32(pre + "AlarmBarSizeX" );
			ReadUInt32(pre + "AlarmBarSizeY" );

		}

		// **************** READ AND CHECK VALUES **********************************************************************

		public static byte ReadByte(string VarName, byte CheckValue)
		{
			byte V = ReadByte(VarName);
			if (V != CheckValue)
			{
				Console.WriteLine("*** Error ***  Value is not: " + CheckValue);
				Errors += VarName + " error, Value is " + V.ToString() + ", not " + CheckValue.ToString() + ".\n";
			}
			return V;
		}

		public static UInt32 ReadUInt16(string VarName, UInt32 CheckValue)
		{
			UInt16 V = ReadUInt16(VarName);
			if (V != CheckValue)
			{
				Console.WriteLine("*** Error ***  Value is not: " + CheckValue);
				Errors += VarName + " error, Value is " + V.ToString() + ", not " + CheckValue.ToString() + ".\n";
			}
			return V;
		}

		public static UInt32 ReadUInt32(string VarName, UInt32 CheckValue)
		{
			UInt32 V = ReadUInt32(VarName);
			if (V != CheckValue)
			{
				Console.WriteLine("*** Error ***  Value is not: " + CheckValue);
				Errors += VarName + " error, Value is " + V.ToString() + ", not " + CheckValue.ToString() + ".\n";
			}
			return V;
		}

		// **************** READ/WRITE VALUES **********************************************************************

		public static byte [] ReadBytes( uint length, string VarName)
		{
			Console.Write(VarName);
			Console.Write(": ");
			byte [] V = binReader.ReadBytes( (int)length);
			Console.WriteLine((BitConverter.ToString(V)).Replace('-', ' ' ) ); //Encoding.Default.GetString(V) );
			if (VarName == changeKey)
			{
				V = Enumerable.Range(0, newValue.Length / 2).Select(x => Convert.ToByte(newValue.Substring(x * 2, 2), 16)).ToArray();
				changed = true;
				Console.WriteLine(">>> Changed to: " + newValue);
			}
			binWriter.Write(V);
			return V;
		}

		public static char ReadChar(string VarName)
		{
			Console.Write(VarName);
			Console.Write(":  ");
			char V = binReader.ReadChar();
			Console.WriteLine("'" + V + "'");
			if (VarName == changeKey)
			{
				V = newValue[0];
				changed = true;
				Console.WriteLine(">>> Changed to: " + newValue);
			}
			binWriter.Write(V);
			return V;
		}

		public static byte ReadByte(string VarName)
		{
			Console.Write(VarName);
			Console.Write(":  ");
			byte V = binReader.ReadByte();
			Console.WriteLine(V.ToString());
			if (VarName == changeKey)
			{
				V = byte.Parse(newValue);
				changed = true;
				Console.WriteLine(">>> Changed to: " + newValue);
			}
			binWriter.Write(V);
			return V;
		}

		public static UInt16 ReadUInt16( string VarName)
		{
			Console.Write( VarName);
			Console.Write(":  ");
			UInt16 V = binReader.ReadUInt16();
			Console.WriteLine( V.ToString());
			if (VarName == changeKey)
			{
				V = UInt16.Parse(newValue);
				changed = true;
				Console.WriteLine(">>> Changed to: " + newValue);
			}
			binWriter.Write(V);
			return V;
		}

		public static double ReadDouble(string VarName)
		{
			Console.Write(VarName);
			Console.Write(":  ");
			double V = binReader.ReadDouble();
			Console.WriteLine(V.ToString());
			if (VarName == changeKey)
			{
				V = double.Parse(newValue);
				changed = true;
				Console.WriteLine(">>> Changed to: " + newValue);
			}
			binWriter.Write(V);
			return V;
		}

		public static float ReadFloat(string VarName)
		{
			Console.Write(VarName);
			Console.Write(":  ");
			float V = binReader.ReadSingle();
			Console.WriteLine(V.ToString());
			if (VarName == changeKey)
			{
				V = float.Parse(newValue);
				changed = true;
				Console.WriteLine(">>> Changed to: " + newValue);
			}
			binWriter.Write(V);
			return V;
		}
		public static UInt32 ReadUInt32(string VarName)
		{
			Console.Write(VarName);
			Console.Write(":  ");
			UInt32 V = binReader.ReadUInt32();
			Console.WriteLine(V.ToString());
			if (VarName == changeKey)
			{
				V = UInt32.Parse(newValue);
				changed = true;
				Console.WriteLine(">>> Changed to: " + newValue);
			}
			binWriter.Write(V);
			return V;
		}

		public static UInt64 ReadUInt64(string VarName)
		{
			Console.Write(VarName);
			Console.Write(":  ");
			UInt64 V = binReader.ReadUInt64();
			Console.WriteLine(V.ToString());
			if (VarName == changeKey)
			{
				V = UInt64.Parse(newValue);
				changed = true;
				Console.WriteLine(">>> Changed to: " + newValue);
			}
			binWriter.Write(V);
			return V;
		}

		// Read Unicode Cstring from Archive
		public static string  ReadUString( string VarName)
		{
			Console.Write(VarName);
			Console.Write(":  ");
			string result = "";
			UInt32 length = 0;

			byte b1 = binReader.ReadByte();
			byte b2 = binReader.ReadByte();
			byte b3 = binReader.ReadByte();
			if ((b1 == 0xff) && (b2 == 0xfe) && (b3 == 0xff))
			{
				length = binReader.ReadByte();
				if (length == 0xff)
				{
					length = binReader.ReadUInt16();
					if (length == 0xffff)
					{
						length = binReader.ReadUInt32();
						Console.Write("  Length: " + (length/2).ToString());
					}
				}
				if (length > 0)
				{
					byte[] str = binReader.ReadBytes((int)length * 2);
					char[] s = Encoding.Unicode.GetChars(str);
					foreach (char c in s)
					{
						result += c;
					}
					Console.WriteLine("\"" + result + "\"");
				}
				else
				{
					Console.WriteLine(":  \"\"");
					result = "";
				}

				// Substitute
				if (VarName == changeKey)
				{
					result = newValue;
					length = (uint)result.Length;
					changed = true;
					Console.WriteLine(">>> Changed to: \"" + newValue + "\"");
				}

				// Write
				if (length > 0)
				{
					byte[] bs = { 0xFF, 0xFE, 0xFF };
					binWriter.Write(bs);

					if (length < 256)
					{
						binWriter.Write((byte)length);
					}
					else
					{
						if (length < 256 * 256)
						{
							binWriter.Write((byte)0xff);
							binWriter.Write((UInt16)length);
						}
						else
						{
							binWriter.Write((UInt16)0xffff);
							binWriter.Write((UInt32)length);
						}
					}
					foreach (char c in result)
					{
						binWriter.Write(c);
						binWriter.Write((byte)0);
					}

				}
				else
				{
					byte[] bs = { 0xFF, 0xFE, 0xFF, 0x00 };
					binWriter.Write(bs);
				}

				return result;
			}
			else
			{
				Console.WriteLine("  Not Unicode Cstring Error");
				Errors += VarName + " error, Not Unicode Cstring.\n";
				return "*** ERROR ***";
			}
		}
	}
}
