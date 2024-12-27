# MagicProxyPrinter

Have you ever thought about building your deck in Magic The Gathering? Have you ever found yourself missing some cards for your newly created deck, but still wanted to give it a try? Do you need some card replacements until your new cards arrive? Or perhaps you want to build a deck before spending real money on it?

If so, then this application is for you! It allows you to generate a printable and editable document with cards from a deck stored online or exported to a text file. With this application, you can easily print your previously created deck and try it out at the table with your friend(s)!

## Features

- Download deck list straight from deck-builder websites like Archidekt, Moxfield or EDHRec via URL 
- Save cards resized and adjusted for printing in editable Word .docx format
- Get deck list from exported file
- Add cards number of times per number of quantity
- Print dual side cards
- Download cards from specified expansion and specific card version
- Support art cards
- Option to download all cards in specific language (cards not found in given language will be replaced with default english language)
- Option to add related tokens from cards in the deck
- Option to group tokens based on name and print only single copies of that card
- Option to store original images alongside created document
- Logs in separate file showing errors in receiving data
- Show % progress status in console app

## Plans

- Support to other deck-builder websites
- Add option to have square corners for all cards
- Produce read-only PDF document instead of Word
- (Technical thing only) Create integration tests
- Create Web version with Blazor
- Host Web version via Github Pages
- Build Web version via Github Actions

## Limitation

### General

- Custom cards are not supported (not implemented and not in plans for now)
- Foil version of non unique card arts are not supported (Scryfall API that provide high quality foil card images except etched foil and unique foiled arts)

### Archidekt

- Download selected cards in specific language not supported (Archidekt does not support card language selection)
- Import based on file exported from Archidekt does not support specific card version (missing *card number* information in exported file)

### EDHRec

- No support for specific version of cards (EDHRec does not provide API and their basic website does not specify what version of card is used)
- Support only deckpreview pages for now

## Usage

Call ArchidektProxyPrinter file (can have .exe extension) from command liner with one of the following options:

    -- deck-file-path  <String>
    -- deck-url  <String>

List of all parameters:
```
Usage: MagicProxyPrint [--deck-file-path <String>] [--deck-url <String>] [--language-code <String>] [--token-copies <Int32>] [--group-tokens] [--output-path <String>] [--output-file-name <String>] [--store-original-images] [--help] [--version]

MagicProxyPrinter

Options:
  --deck-file-path <String>      Filepath to exported deck from Archidekt
  --deck-url <String>            URL link to deck in Archidekt
  --language-code <String>       Set language for all cards to print
  --token-copies <Int32>         Number of copy for each token
  --group-tokens                 Group tokens based on the name
  --output-path <String>         Directory path to output file(s)
  --output-file-name <String>    Filename of the output word file
  --store-original-images        Flag to store original images in the same folder as output file
  -h, --help                     Show help message
  --version                      Show version
  ```

## Installation

Download existing file from Github Releases section of this project https://github.com/Furrman/MagicProxyPrinter/releases and put it anywhere you want. You can add path to this console app in your PATH environment variable.

## Instruction

You need to have .NET8 SDK installed to build this solution. You can use Visual Studio, Visual Studio Code IDE or just simple terminal. The command to run the build is:

`dotnet build`

To publish your application use:

`dotnet publish --configuration Release`
