// Copyright (c) Files Community
// Licensed under the MIT License.

using System.Runtime.CompilerServices;

namespace Files.App.Services.Settings
{
	internal sealed partial class LayoutSettingsService : BaseObservableJsonSettings, ILayoutSettingsService
	{
		public LayoutSettingsService(ISettingsSharingContext settingsSharingContext)
		{
			// Register root
			RegisterSettingsContext(settingsSharingContext);
		}

		public bool SyncFolderPreferencesAcrossDirectories
		{
			get => Get(false);
			set => Set(value);
		}

		public FolderLayoutModes DefaultLayoutMode
		{
			get => (FolderLayoutModes)Get((long)FolderLayoutModes.Adaptive);
			set => Set((long)value);
		}

		public SortOption DefaultSortOption
		{
			get => (SortOption)Get((long)SortOption.Name);
			set => Set((long)value);
		}

		public SortDirection DefaultDirectorySortDirection
		{
			get => (SortDirection)Get((long)SortDirection.Ascending);
			set => Set((long)value);
		}

		public bool DefaultSortDirectoriesAlongsideFiles
		{
			get => Get(false);
			set => Set(value);
		}

		public bool DefaultSortFilesFirst
		{
			get => Get(false);
			set => Set(value);
		}

		public GroupOption DefaultGroupOption
		{
			get => (GroupOption)Get((long)GroupOption.None);
			set => Set((long)value);
		}

		public SortDirection DefaultDirectoryGroupDirection
		{
			get => (SortDirection)Get((long)SortDirection.Ascending);
			set => Set((long)value);
		}

		public GroupByDateUnit DefaultGroupByDateUnit
		{
			get => (GroupByDateUnit)Get((long)GroupByDateUnit.Year);
			set => Set((long)value);
		}

		private double GetColumnWidth(double defaultValue, [CallerMemberName] string propertyName = "")
		{
			var val = Get(defaultValue, propertyName);
			return val <= 10d ? defaultValue : val;
		}

		private void SetColumnWidth(double value, bool condition = true, double minWidth = 50d, [CallerMemberName] string propertyName = "")
		{
			if (condition)
				Set(Math.Max(minWidth, value), propertyName);
		}

		public double GitStatusColumnWidth
		{
			get => GetColumnWidth(80d);
			set => SetColumnWidth(value, ShowGitStatusColumn);
		}

		public double GitLastCommitDateColumnWidth
		{
			get => GetColumnWidth(140d);
			set => SetColumnWidth(value, ShowGitLastCommitDateColumn);
		}

		public double GitLastCommitMessageColumnWidth
		{
			get => GetColumnWidth(140d);
			set => SetColumnWidth(value, ShowGitLastCommitMessageColumn);
		}

		public double GitCommitAuthorColumnWidth
		{
			get => GetColumnWidth(140d);
			set => SetColumnWidth(value, ShowGitCommitAuthorColumn);
		}

		public double GitLastCommitShaColumnWidth
		{
			get => GetColumnWidth(80d);
			set => SetColumnWidth(value, ShowGitLastCommitShaColumn);
		}

		public double TagColumnWidth
		{
			get => GetColumnWidth(140d);
			set => SetColumnWidth(value, ShowFileTagColumn);
		}

		public double NameColumnWidth
		{
			get => GetColumnWidth(240d);
			set => SetColumnWidth(value, minWidth: 80d);
		}

		public double DateModifiedColumnWidth
		{
			get => GetColumnWidth(200d);
			set => SetColumnWidth(value, ShowDateColumn);
		}

		public double TypeColumnWidth
		{
			get => GetColumnWidth(140d);
			set => SetColumnWidth(value, ShowTypeColumn);
		}

		public double DateCreatedColumnWidth
		{
			get => GetColumnWidth(200d);
			set => SetColumnWidth(value, ShowDateCreatedColumn);
		}

		public double SizeColumnWidth
		{
			get => GetColumnWidth(100d);
			set => SetColumnWidth(value, ShowSizeColumn);
		}

		public double DateDeletedColumnWidth
		{
			get => GetColumnWidth(200d);
			set => SetColumnWidth(value, ShowDateDeletedColumn);
		}

		public double PathColumnWidth
		{
			get => GetColumnWidth(200d);
			set => SetColumnWidth(value, ShowPathColumn);
		}

		public double OriginalPathColumnWidth
		{
			get => GetColumnWidth(200d);
			set => SetColumnWidth(value, ShowOriginalPathColumn);
		}

		public double SyncStatusColumnWidth
		{
			get => GetColumnWidth(50d);
			set => SetColumnWidth(value, ShowSyncStatusColumn, minWidth: 30d);
		}

		public bool ShowDateColumn
		{
			get => Get(true);
			set => Set(value);
		}

		public bool ShowDateCreatedColumn
		{
			get => Get(false);
			set => Set(value);
		}

		public bool ShowTypeColumn
		{
			get => Get(true);
			set => Set(value);
		}

		public bool ShowSizeColumn
		{
			get => Get(true);
			set => Set(value);
		}

		public bool ShowGitStatusColumn
		{
			get => Get(false);
			set => Set(value);
		}

		public bool ShowGitLastCommitDateColumn
		{
			get => Get(false);
			set => Set(value);
		}

		public bool ShowGitLastCommitMessageColumn
		{
			get => Get(false);
			set => Set(value);
		}

		public bool ShowGitCommitAuthorColumn
		{
			get => Get(false);
			set => Set(value);
		}

		public bool ShowGitLastCommitShaColumn
		{
			get => Get(false);
			set => Set(value);
		}

		public bool ShowFileTagColumn
		{
			get => Get(true);
			set => Set(value);
		}

		public bool ShowDateDeletedColumn
		{
			get => Get(true);
			set => Set(value);
		}

		public bool ShowPathColumn
		{
			get => Get(true);
			set => Set(value);
		}

		public bool ShowOriginalPathColumn
		{
			get => Get(true);
			set => Set(value);
		}

		public bool ShowSyncStatusColumn
		{
			get => Get(true);
			set => Set(value);
		}

		public bool AutoSizeColumnsInDetailsLayout
		{
			get => Get(false);
			set => Set(value);
		}

		public DetailsViewSizeKind DetailsViewSize
		{
			get => Get(DetailsViewSizeKind.Small);
			set => Set(value);
		}

		public ListViewSizeKind ListViewSize
		{
			get => Get(ListViewSizeKind.Small);
			set => Set(value);
		}

		public CardsViewSizeKind CardsViewSize
		{
			get => Get(CardsViewSizeKind.Small);
			set => Set(value);
		}

		public GridViewSizeKind GridViewSize
		{
			get => Get(GridViewSizeKind.Large);
			set => Set(value);
		}

		public ColumnsViewSizeKind ColumnsViewSize
		{
			get => Get(ColumnsViewSizeKind.Small);
			set => Set(value);
		}

		protected override void RaiseOnSettingChangedEvent(object sender, SettingChangedEventArgs e)
		{
			base.RaiseOnSettingChangedEvent(sender, e);
		}
	}
}
