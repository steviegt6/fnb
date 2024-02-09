use clap::{Parser, Subcommand};

#[derive(Subcommand, Debug)]
enum Commands {
    #[command(
        name = "tmod extract",
        about = "Extracts a .tmod file archive into a directory"
    )]
    TmodExtract {
        #[clap(help = "The .tmod file path")]
        path: String,

        #[clap(
            short = 'o',
            long = "out-dir",
            help = "The output directory to extract the .tmod file into"
        )]
        out_dir: Option<String>,
        file: Option<String>,
        list: bool,
        pure: bool,
    },

    #[command(
        name = "tmod extract-local",
        about = "Extracts a mod installed locally in your tModLoader installation"
    )]
    TmodExtractLocal,

    #[command(
        name = "tmod extract-workshop",
        about = "Extracts a mod installed through the Steam Workshop"
    )]
    TmodExtractWorkshop,

    #[command(
        name = "tmod list-locals",
        about = "Lists known, locally-installed .tmod files"
    )]
    TmodListLocals,

    #[command(
        name = "tmod list-workshop",
        about = "Lists .tmod files installed through the Steam Workshop"
    )]
    TmodListWorkshop,

    #[command(
        name = "tmod pack",
        about = "Pack a directory into a .tmod file archive"
    )]
    TmodPack,

    #[command(
        name = "xnb extract",
        about = "Extracts an XNB file or directory of XNB files into their original formats"
    )]
    XnbExtract,

    #[command(
        name = "xnb list-formats",
        about = "Lists the known formats that can be extracted from XNB files"
    )]
    XnbListFormats,

    #[command(
        name = "xnb pack",
        about = "Packs a file or directory of files into XNB files"
    )]
    XnbPack,
}

#[derive(Parser, Debug)]
struct App {
    #[command(subcommand)]
    command: Option<Commands>,
}

fn main() {
    let app = App::parse();
}
