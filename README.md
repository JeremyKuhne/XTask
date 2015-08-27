# xTask Framework #

xTask is a set of functionality that is useful in creating command line and other .NET utilities. The key features are around:

- Long path support
- Abstractions for IO (file system, console, etc.)
- Command line parsing
- Logging 
- MSBuild integration
- Settings manipulation

The goal of this project is to allow easy creation of useful tool code that can be reused in different workflows (MSBuild and Command Prompt for now, PowerShell and others in the future).


## Features ##
Here is a list of _some_ of the interesting / useful functionality currently in the framework:
- External Abstractions
	- Interfaces and wrappers defined to allow easy mocking for tests
	- Enables extended support (IFileSystem has a long path supporting implementation)
- Powerful IO
	- Implicit long path support
	- Access to alternate streams
	- Support for \\?\ and \\.\ syntax
	- Volume management access
- Parsing
	- Flexible consumption of options (ask for an option as a bool and it will be converted for you)
	- Any options from files (/ignore:@ignore.list)
- Logging
	- Easy table logging
	- Specification of output vs. informational logging
	- RichText, HTML, CSV, text, and XmlSpreadsheet logging support
	- Clipboard integration (with table formatting for Excel, Word, etc.)
- Settings
	- Save/pull settings from any level of config file
- MSBuild
	- Easy exposure to MSBuild with full access to commands and options
	- Output goes directly through MSBuild logging
	- Ability to push items with metadata to MSBuild output
- Utility
	- Safe and simple Registry access
	- Easy type conversion
	- Rich assembly resolution support
	- Temporary file handling (including robust cleanup)
 
## Documentation ##
Standalone is forthcoming. Liberal comments are throughout the code.
## Contributing ##
Contribution is welcome for all changes that are free to be put under the current Copyright and license. Other changes will be considered as well.
## License ##
Released under the MIT license, see the LICENSE file for more information.