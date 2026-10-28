#!/usr/bin/env bash
# Run on the GitHub Linux runner after select-gameci-java and setup-android.
# Copy portable Linux tools under the directory GameCI mounts into Docker.
set -euo pipefail

: "${GITHUB_WORKSPACE:?GITHUB_WORKSPACE is required}"
: "${JAVA_HOME:?Run select-gameci-java.sh first}"
nfsdk_android_source="${ANDROID_SDK_ROOT:-${ANDROID_HOME:-}}"
: "${nfsdk_android_source:?Run android-actions/setup-android first}"
nfsdk_tools_root="$GITHUB_WORKSPACE/.ci-tools"
nfsdk_sdk_target="$nfsdk_tools_root/android-sdk"
mkdir -p "$nfsdk_tools_root/jdk" "$nfsdk_sdk_target"

test -f "$JAVA_HOME/release"
nfsdk_java_major="$(sed -nE 's/^JAVA_VERSION="([0-9]+).*$/\1/p' "$JAVA_HOME/release" | head -n 1)"
if [[ ! "$nfsdk_java_major" =~ ^[0-9]+$ ]] || (( nfsdk_java_major < 17 || nfsdk_java_major > 23 )); then
  echo 'The selected runner JDK must be compatible with AGP/Gradle: Java 17-23.' >&2
  exit 1
fi
nfsdk_script_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
python3 "$nfsdk_script_root/stage-gameci-jdk.py" \
  --source "$JAVA_HOME" --destination "$nfsdk_tools_root/jdk"

for nfsdk_relative in \
  platforms/android-36 \
  build-tools/36.1.0 \
  cmdline-tools/6.0 \
  platform-tools \
  licenses; do
  if [[ ! -d "$nfsdk_android_source/$nfsdk_relative" ]]; then
    echo "Missing SDK component: $nfsdk_relative. Check the setup-android packages input." >&2
    exit 1
  fi
  mkdir -p "$nfsdk_sdk_target/$nfsdk_relative"
  cp -a "$nfsdk_android_source/$nfsdk_relative/." "$nfsdk_sdk_target/$nfsdk_relative/"
done

nfsdk_gradle_zip="$nfsdk_tools_root/gradle-8.13-bin.zip"
curl --fail --location --retry 3 --silent --show-error \
  https://services.gradle.org/distributions/gradle-8.13-bin.zip -o "$nfsdk_gradle_zip"
nfsdk_gradle_sha="$(curl --fail --location --retry 3 --silent --show-error \
  https://services.gradle.org/distributions/gradle-8.13-bin.zip.sha256)"
printf '%s  %s\n' "$nfsdk_gradle_sha" "$nfsdk_gradle_zip" | sha256sum -c
unzip -q -o "$nfsdk_gradle_zip" -d "$nfsdk_tools_root"
rm -f "$nfsdk_gradle_zip"

# Keep the CI-only tool directory out of GameCI's dirty checkout check.
nfsdk_git_exclude="$(git -C "$GITHUB_WORKSPACE" rev-parse --git-path info/exclude)"
if [[ "$nfsdk_git_exclude" != /* ]]; then
  nfsdk_git_exclude="$GITHUB_WORKSPACE/$nfsdk_git_exclude"
fi
printf '\n/.ci-tools/\n' >> "$nfsdk_git_exclude"

test -x "$nfsdk_tools_root/jdk/bin/java"
test -r "$nfsdk_tools_root/jdk/lib/security/cacerts"
test -s "$nfsdk_tools_root/jdk/lib/security/cacerts"
test ! -L "$nfsdk_tools_root/jdk/lib/security/cacerts"
test -f "$nfsdk_tools_root/gradle-8.13/lib/gradle-launcher-8.13.jar"
test -x "$nfsdk_sdk_target/cmdline-tools/6.0/bin/sdkmanager"
test -f "$nfsdk_sdk_target/platforms/android-36/android.jar"
test -x "$nfsdk_sdk_target/build-tools/36.1.0/aapt2"

echo 'GameCI container tool paths prepared:'
echo '  JDK:    /github/workspace/.ci-tools/jdk'
echo '  Gradle: /github/workspace/.ci-tools/gradle-8.13'
echo '  SDK:    /github/workspace/.ci-tools/android-sdk'
"$nfsdk_tools_root/jdk/bin/java" -version
