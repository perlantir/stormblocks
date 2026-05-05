fastlane documentation
----

# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```sh
xcode-select --install
```

For _fastlane_ installation instructions, see [Installing _fastlane_](https://docs.fastlane.tools/#installing-fastlane)

# Available Actions

## iOS

### ios create_app_record

```sh
[bundle exec] fastlane ios create_app_record
```

Create the missing App Store Connect app record and enable Game Center capability.

### ios upload_testflight

```sh
[bundle exec] fastlane ios upload_testflight
```

Upload the already exported Storm Blocks IPA to TestFlight.

### ios release_candidate_upload

```sh
[bundle exec] fastlane ios release_candidate_upload
```

Create the App Store Connect app record if needed, then upload the exported IPA.

----

This README.md is auto-generated and will be re-generated every time [_fastlane_](https://fastlane.tools) is run.

More information about _fastlane_ can be found on [fastlane.tools](https://fastlane.tools).

The documentation of _fastlane_ can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
