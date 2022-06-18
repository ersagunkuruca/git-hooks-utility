# Git Hooks Utility

Automatically shared git hooks for Unity projects.

This is largely based on the original project at: https://github.com/Babilinski/git-hook-utility 

It attempts improve upon the original project in a few points: shared, auto-installed, auto-updated hooks, cleaner installation.

## Installation

Add this to your `manifest.json`:
> "com.ersagun.git-hooks-utility": "https://github.com/ersagunkuruca/git-hooks-utility.git"

Or go to `Package Manager > + > Add package from git URL` and paste the git URL.

Or clone the repository into your project's `Packages` folder.

## Configuration
After installation go to `Tools/Git Hooks Utility/Select Configuration Asset` to see the current configuration. Changing any of the files listed in the configuration file automatically reinstalls the hooks into your repository `.git/hooks` directory.

The default hooks are from: https://github.com/doitian/unity-git-hooks

## Known issues / Roadmap
- Currently this package cannot uninstall the hooks it installs, and when you install the package and the hooks, any of your existing hooks are overwritten. Enable option on the configuration asset just enables/disables auto-installation across repositories. **Backing up currently existing hooks and removing unused hooks are left to the user of the package. Use at your own risk.**
- Your project has to be in your git repository root directory. Although the example hooks support customizing this, this package doesn't yet do anything to support this.
- First installation still seems to have a few issues. Usually a single Unity reload fixes the situation.