const axios = require("axios");
const fs = require("fs-extra");
const path = require("path");
const toml = require("toml");
const extract = require("extract-zip");

async function downloadFile(url, outputPath) {
	const writer = fs.createWriteStream(outputPath);
	const response = await axios({
		url,
		method: "GET",
		responseType: "stream",
	});

	response.data.pipe(writer);

	return new Promise((resolve, reject) => {
		writer.on("finish", resolve);
		writer.on("error", reject);
	});
}

async function extractDlls(zipPath, outputDir) {
	const extractPath = path.join(outputDir, "temp");
	await extract(zipPath, { dir: extractPath });

	async function moveDllFiles(dir) {
		const files = await fs.readdir(dir);
		for (const file of files) {
			const filePath = path.join(dir, file);
			const fileStat = await fs.stat(filePath);
			if (fileStat.isFile() && path.extname(file) === ".dll") {
				await fs.move(filePath, path.join(outputDir, path.basename(file)), {
					overwrite: true,
				});
			} else if (fileStat.isDirectory()) {
				await moveDllFiles(filePath);
			}
		}
	}

	await moveDllFiles(extractPath);

	// Clean up temp directory
	await fs.remove(extractPath);
}

async function isFileDownloaded(url, outputPath) {
	if (!fs.existsSync(outputPath)) {
		return false;
	}

	const downloadedFileStats = fs.statSync(outputPath);
	const response = await axios.head(url);
	const remoteFileSize = parseInt(response.headers["content-length"], 10);

	return downloadedFileStats.size === remoteFileSize;
}

async function main() {
	const [tomlFilePath, libraryDir] = process.argv.slice(2);

	if (!tomlFilePath || !libraryDir) {
		console.error(
			"Usage: node downloader.js <path to libraries.toml> <library directory>"
		);
		process.exit(1);
	}

	const absoluteTomlFilePath = path.resolve(tomlFilePath);
	const absoluteLibraryDir = path.resolve(libraryDir);

	const tomlContent = fs.readFileSync(absoluteTomlFilePath, "utf-8");
	const config = toml.parse(tomlContent);

	for (const lib of config.library) {
		const zipPath = path.join(absoluteLibraryDir, `${lib.name}.zip`);
		try {
			const isDownloaded = await isFileDownloaded(lib.url, zipPath);

			if (!isDownloaded) {
				console.log(`Downloading ${lib.url}...`);
				await downloadFile(lib.url, zipPath);
				console.log(`Downloaded ${lib.name}`);
			} else {
				console.log(`${lib.name} is already downloaded.`);
			}

			console.log(`Extracting ${zipPath}...`);
			await extractDlls(zipPath, absoluteLibraryDir);
			console.log(`Extracted ${lib.name}`);
		} catch (error) {
			console.error(`Failed to process ${lib.name}:`, error);
		}
	}
}

main();
