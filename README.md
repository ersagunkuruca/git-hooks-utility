# Git Hooks Utility

Automatically shared git hooks for Unity projects.

This is largely based on the original project at: https://github.com/Babilinski/git-hook-utility 

It (humbly) improves upon the original project in a few points: shared, auto-installed, auto updated hooks, cleaner installation.

## Installation

Add this to your `packages.json`:
> "com.ersagun.git-hooks-utility": "https://github.com/ersagunkuruca/git-hooks-utility.git"

## Configuration
After installation go to `Tools/Git Hooks Utility/Select Configuration Asset` to see the current configuration. Changing any of the files listed in the configuration file automatically reinstalls the hooks into your repository `.git/hooks` directory.

The default hooks are from: https://github.com/doitian/unity-git-hooks