# m4a_split_by_chapters
Split single m4a file by chapters to mp3 files.


## Prerequisites

- FFMPEG https://ffmpeg.org/ installed on your system and added to your PATH.  
You can use standard windows installation.
- .NET SDK installed on your system. You can download it from https://dotnet.microsoft.com/download.

## Usage

1. Clone the repository:
    ```sh
    git clone https://github.com/Dav-Sim/m4a_split_by_chapters.git
    ```
2. Navigate to the project directory:
    ```sh
    cd m4a_split_by_chapters/AudioChapterSplitter
    ```
3. Build the project:
    ```sh
    dotnet build
    ```
4. Run the application:
    ```sh
    dotnet run --project AudioChapterSplitter -- <path_to_m4a_file> [optional output directory]
    ```
5. The application will split the provided m4a file into separate mp3 files based on the chapters.

## Contributing

Contributions are welcome! Please fork the repository and create a pull request with your changes.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.