# Audible Audio Converter Cross Platform

A simple and efficient audio converter to convert .aax files (Audible audiobooks) into different audio formats with varying quality levels.

## Features
-	**Convert Audible Audiobooks**: Convert .aax files to popular formats like .mp3, .m4b, and .flac.
-   **Multiple Quality Levels**:Choose from various quality levels from 32 kbps (very low) to 320 kbps (very high).
-   **Platform**: Developed using .NET 8 and Uno Skia for cross-platform compatibility (Linux and MacOS).

## Requirements
-   **.NET 8**: Make sure you have .NET 8 installed. Download .NET 8
-   **FFmpeg**: This tool requires FFmpeg for the conversion process. 

## Usage
1.  **Select AAX File**: Click on the Choose AAX File button and select the .aax file you want to convert.
2.  **Select Output Folder**: Click on the Extract Folder button to select the destination folder for the converted file.
3.  **Choose Format and Quality**: Use the dropdown menus to select the desired output format and quality level.
4.  **Start Conversion**: Click the Convert button to start the conversion process. The progress will be shown on the progress bar.
5.  **Cancel Conversion**: You can cancel the conversion process by clicking the Cancel button.
6.  **Open Output Folder**: Once the conversion is completed, click the Open Output Folder button to view the converted files.

## Knwon Issues
-   **Large Files**: Conversion of very large .aax files might take considerable time depending on the system's performance.
-   **Cross-Platform Limitations**: Although developed to be cross-platform, certain functionalities might behave differently on different operating systems (only working on Linux and  MacOS (not tested yet)).

## Contribution
Contributions are welcome! Please open an issue or submit a pull request with your improvements.


## Acknowledgements
-   **FFmpeg**: Used for audio conversion.
-   **Uno Platform**: For enabling cross-platform development.
