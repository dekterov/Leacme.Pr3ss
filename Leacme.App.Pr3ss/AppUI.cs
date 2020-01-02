// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Ionic.Zip;
using Ionic.Zlib;
using Leacme.Lib.Pr3ss;

namespace Leacme.App.Pr3ss {

	public class AppUI {

		private StackPanel rootPan = (StackPanel)Application.Current.MainWindow.Content;
		private Library lib = new Library();

		public AppUI() {

			var compLb = App.TextBlock;
			compLb.TextAlignment = TextAlignment.Center;
			compLb.Background = Brushes.LightGray;
			compLb.Text = "Compress Directory to Zip";

			var oFc = App.HorizontalFieldWithButton;
			oFc.holder.HorizontalAlignment = HorizontalAlignment.Center;
			oFc.label.Text = "Directory to compress:";
			oFc.field.IsReadOnly = true;
			oFc.field.Width = 500;
			oFc.button.Content = "Open...";

			var hCc = App.HorizontalStackPanel;
			hCc.HorizontalAlignment = HorizontalAlignment.Center;

			var ccl = App.ComboBoxWithLabel;
			ccl.label.Text = "Compression level:";
			ccl.comboBox.Items = Enum.GetValues(typeof(CompressionLevel));
			ccl.comboBox.Width = 100;
			ccl.comboBox.SelectedIndex = 8;

			var ccm = App.ComboBoxWithLabel;
			ccm.label.Text = "Compression method:";
			ccm.comboBox.Items = Enum.GetValues(typeof(CompressionMethod));
			ccm.comboBox.Width = 100;
			ccm.comboBox.SelectedIndex = 1;

			var pteLb = App.TextBlock;
			pteLb.Text = "Password to encrypt:";
			var pteFl = App.TextBox;

			hCc.Children.AddRange(new List<IControl> { ccl.holder, ccm.holder, pteLb, pteFl });

			var aFc = App.HorizontalFieldWithButton;
			aFc.holder.HorizontalAlignment = HorizontalAlignment.Center;
			aFc.label.Text = "Output zip archive:";
			aFc.field.IsReadOnly = true;
			aFc.field.Width = 500;
			aFc.button.Content = "Save...";

			var hCs = App.HorizontalStackPanel;
			hCs.HorizontalAlignment = HorizontalAlignment.Center;

			var prLb = App.TextBlock;
			prLb.Text = "Progress:";

			var prPerc = App.TextBlock;
			prPerc.Text = 0.ToString();

			var prUt = App.TextBlock;
			prUt.Text = "% completed";

			hCs.Children.AddRange(new List<IControl> { prLb, prPerc, prUt });

			oFc.button.Click += async (z, zz) => {
				var ftcp = await OpenFolder();
				oFc.field.Text = string.Empty;
				if (!string.IsNullOrWhiteSpace(ftcp)) {
					oFc.field.Text = ftcp;
					prPerc.Text = 0.ToString();
				}
			};

			aFc.button.Click += async (z, zz) => {
				var outP = await SaveFile();
				if (!string.IsNullOrWhiteSpace(outP)) {
					aFc.field.Text = outP;
					prPerc.Text = 0.ToString();
				}
			};

			var saveIndicator = new Progress<int>(z => {
				prPerc.Text = z.ToString();
			});

			var bCb = App.Button;
			bCb.Content = "Begin Compress";
			bCb.Click += async (z, zz) => {
				if (!string.IsNullOrWhiteSpace(oFc.field.Text) && !string.IsNullOrWhiteSpace(aFc.field.Text)) {
					string tempPass = null;
					if (!string.IsNullOrEmpty(pteFl.Text)) {
						tempPass = pteFl.Text;
					}
					await lib.ZipFilesAsync(new HashSet<Uri> { new Uri(oFc.field.Text) }, new Uri(aFc.field.Text), (CompressionLevel)ccl.comboBox.SelectedItem, (CompressionMethod)ccm.comboBox.SelectedItem, tempPass, saveIndicator);
				}
			};

			var decompLb = App.TextBlock;
			decompLb.TextAlignment = TextAlignment.Center;
			decompLb.Background = Brushes.LightGray;
			decompLb.Text = "Decompress a Zip Archive";

			var pFc = App.HorizontalFieldWithButton;
			pFc.holder.HorizontalAlignment = HorizontalAlignment.Center;
			pFc.label.Text = "Open zip archive:";
			pFc.field.IsReadOnly = true;
			pFc.field.Width = 500;
			pFc.button.Content = "Open...";

			var rFc = App.HorizontalFieldWithButton;
			rFc.holder.HorizontalAlignment = HorizontalAlignment.Center;
			rFc.label.Text = "Extract to directory:";
			rFc.field.IsReadOnly = true;
			rFc.field.Width = 500;
			rFc.button.Content = "Choose...";

			var ptdLb = App.TextBlock;
			ptdLb.Text = "Password to decrypt:";
			var ptdFl = App.TextBox;

			var ovLb = App.TextBlock;
			ovLb.Text = "Overwrite files:";
			var ovCb = App.CheckBox;
			ovCb.Background = Brushes.White;

			var hCd = App.HorizontalStackPanel;
			hCd.HorizontalAlignment = HorizontalAlignment.Center;

			hCd.Children.AddRange(new List<IControl> { ptdLb, ptdFl, new Control { Width = 20 }, ovLb, ovCb });

			var dhCs = App.HorizontalStackPanel;
			dhCs.HorizontalAlignment = HorizontalAlignment.Center;

			var dprLb = App.TextBlock;
			dprLb.Text = "Progress:";

			var dprPerc = App.TextBlock;
			dprPerc.Text = 0.ToString();

			var dprUt = App.TextBlock;
			dprUt.Text = "% completed";

			dhCs.Children.AddRange(new List<IControl> { dprLb, dprPerc, dprUt });

			pFc.button.Click += async (z, zz) => {
				var outP = await OpenFile();
				if (outP.Any()) {
					pFc.field.Text = outP.First();
					dprPerc.Text = 0.ToString();
				}
			};

			rFc.button.Click += async (z, zz) => {
				var ftcp = await OpenFolder();
				rFc.field.Text = string.Empty;
				if (!string.IsNullOrWhiteSpace(ftcp)) {
					rFc.field.Text = ftcp;
					dprPerc.Text = 0.ToString();
				}
			};

			var extractIndicator = new Progress<int>(z => {
				dprPerc.Text = z.ToString();
			});

			var dbCb = App.Button;
			dbCb.Content = "Begin Decompress";
			dbCb.Click += async (z, zz) => {
				if (!string.IsNullOrWhiteSpace(pFc.field.Text) && !string.IsNullOrWhiteSpace(rFc.field.Text)) {
					string tempPass = null;
					if (!string.IsNullOrEmpty(ptdLb.Text)) {
						tempPass = ptdLb.Text;
					}
					await lib.ExtractArchiveAsync(new Uri(pFc.field.Text), new Uri(rFc.field.Text), tempPass, ovCb.IsChecked == true, extractIndicator);
				}
			};

			rootPan.Children.AddRange(new List<IControl> { compLb, new Control { Height = 20 }, oFc.holder, hCc, aFc.holder, hCs, bCb, new Control { Height = 20 }, decompLb, new Control { Height = 20 }, pFc.holder, hCd, rFc.holder, dhCs, dbCb });

		}

		private async Task<IEnumerable<string>> OpenFile() {
			var dialog = new OpenFileDialog() {
				Title = "Open Zip Archive...",
				InitialDirectory = Directory.GetCurrentDirectory(),
				AllowMultiple = false,
				Filters = new List<FileDialogFilter> { new FileDialogFilter() { Name = "Zip Archive", Extensions = new List<string> { "zip" } } }
			};
			var res = await dialog.ShowAsync(Application.Current.MainWindow);
			return (res?.Any() == true) ? res : Enumerable.Empty<string>();
		}

		private async Task<string> OpenFolder() {
			var dialog = new OpenFolderDialog() {
				Title = "Select Directory to Compress...",
				InitialDirectory = Directory.GetCurrentDirectory(),
			};
			var res = await dialog.ShowAsync(Application.Current.MainWindow);
			return (!string.IsNullOrWhiteSpace(res)) ? res : string.Empty;
		}

		private async Task<string> SaveFile() {
			var dialog = new SaveFileDialog() {
				Title = "Open Zip Archive...",
				InitialDirectory = Directory.GetCurrentDirectory(),
				DefaultExtension = "zip",
			};
			var res = await dialog.ShowAsync(Application.Current.MainWindow);
			return (!string.IsNullOrWhiteSpace(res)) ? res : string.Empty;
		}
	}
}