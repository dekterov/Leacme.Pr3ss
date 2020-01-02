// Copyright (c) 2017 Leacme (http://leac.me). View LICENSE.md for more information.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Ionic.Zip;
using Ionic.Zlib;

namespace Leacme.Lib.Pr3ss {

	public class Library {

		public Library() {

		}

		/// <summary>
		/// Compress a list of files into a .zip archive.
		/// /// </summary>
		/// <param name="filenamesToCompress">Files to add to the archive, absolute paths.</param>
		/// <param name="outputArchiveFilename">The name of the zip archive to be created, absolute path.</param>
		/// <param name="compressionLevel">The zip archive compression level.</param>
		/// <param name="compressionMethod">The zip archive compression method.</param>
		/// <param name="password">Optional password for archive encryption.</param>
		/// <param name="saveProgress">A callback to track the compression progress.</param>
		/// <returns></returns>
		public async Task ZipFilesAsync(HashSet<Uri> filenamesToCompress, Uri outputArchiveFilename, CompressionLevel compressionLevel, CompressionMethod compressionMethod, string password = null, IProgress<int> saveProgress = null) {

			if (File.Exists(outputArchiveFilename.LocalPath)) {
				throw new FileNotFoundException("Archive already exists and overwriteArchive is set to false.");
			} else {
				await Task.Run(() => {
					using (var zipFile = new ZipFile()) {
						if (!string.IsNullOrWhiteSpace(password)) {
							zipFile.Password = password;
							zipFile.Encryption = EncryptionAlgorithm.WinZipAes256;
						}
						zipFile.CompressionLevel = compressionLevel;
						zipFile.CompressionMethod = compressionMethod;

						foreach (var itemToAdd in filenamesToCompress.Select(z => z.LocalPath)) {
							if (Directory.Exists(itemToAdd)) {
								zipFile.AddDirectory(itemToAdd, Path.GetFileName(itemToAdd));
							} else if (File.Exists(itemToAdd)) {
								zipFile.AddFile(itemToAdd, "");
							}
						}

						if (saveProgress != null) {
							zipFile.SaveProgress += (z, zz) => {
								var percent = (int)(1.0d / zz.TotalBytesToTransfer * zz.BytesTransferred * 100.0d);
								if (percent >= 0 && percent <= 100) {
									saveProgress.Report(percent);
								}
							};
						}
						zipFile.Save(outputArchiveFilename.LocalPath);
					}
				});
			}
		}

		/// <summary>
		/// Decompress a .zip archive into a directory.
		/// /// </summary>
		/// <param name="inputArchiveFilename">The name of the zip archive to extract, absolute path.</param>
		/// <param name="outputFilesDirectory">The name of the directory to which to extract the archive, absolute path.</param>
		/// <param name="password">Optional password to decrypt the archive.</param>
		/// <param name="overwriteFiles">Overwrite the files in the output directory.</param>
		/// <param name="extractProgress">A callback to track the extraction progress.</param>
		/// <returns></returns>
		public async Task ExtractArchiveAsync(Uri inputArchiveFilename, Uri outputFilesDirectory, string password = null, bool overwriteFiles = false, IProgress<int> extractProgress = null) {
			await Task.Run(() => {
				using (var zipFile = ZipFile.Read(inputArchiveFilename.LocalPath)) {
					if (!string.IsNullOrWhiteSpace(password)) {
						zipFile.Password = password;
					}

					if (overwriteFiles) {
						zipFile.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
					} else {
						zipFile.ExtractExistingFile = ExtractExistingFileAction.DoNotOverwrite;
					}

					if (extractProgress != null) {
						zipFile.ExtractProgress += (z, zz) => {
							var percent = (int)(1.0d / zz.TotalBytesToTransfer * zz.BytesTransferred * 100.0d);
							if (percent >= 0 && percent <= 100) {
								extractProgress.Report(percent);
							}
						};
					}
					zipFile.ExtractAll(outputFilesDirectory.LocalPath);
				}
			});
		}

	}
}