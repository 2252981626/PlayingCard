#!/usr/bin/env bash
# Select an installed runner JDK compatible with AGP 8.13.1 and Gradle 8.13.
set -euo pipefail

nfsdk_selected_java=''
nfsdk_selected_major=''
nfsdk_try_java() {
  local candidate="${1:-}" major
  [[ -n "$candidate" && -f "$candidate/release" && -x "$candidate/bin/java" && -x "$candidate/bin/javac" ]] || return 1
  major="$(sed -nE 's/^JAVA_VERSION="([0-9]+).*$/\1/p' "$candidate/release" | head -n 1)"
  [[ "$major" =~ ^[0-9]+$ ]] || return 1
  (( major >= 17 && major <= 23 )) || return 1
  nfsdk_selected_java="$candidate"
  nfsdk_selected_major="$major"
}

if ! nfsdk_try_java "${JAVA_HOME:-}"; then
  while IFS= read -r nfsdk_variable; do
    if nfsdk_try_java "${!nfsdk_variable:-}"; then break; fi
  done < <(compgen -A variable JAVA_HOME_ || true)
fi

if [[ -z "$nfsdk_selected_java" ]]; then
  for nfsdk_candidate in /usr/lib/jvm/*; do
    if nfsdk_try_java "$nfsdk_candidate"; then break; fi
  done
fi

if [[ -z "$nfsdk_selected_java" ]]; then
  echo '::error::No installed JDK 17-23 found. AGP 8.13.1 cannot run on JDK 11; use a runner with a compatible JDK or install one before this step.' >&2
  exit 1
fi

: "${GITHUB_ENV:?GITHUB_ENV is required}"
: "${GITHUB_PATH:?GITHUB_PATH is required}"
printf 'JAVA_HOME=%s\n' "$nfsdk_selected_java" >> "$GITHUB_ENV"
printf '%s/bin\n' "$nfsdk_selected_java" >> "$GITHUB_PATH"
printf 'Selected installed JDK %s: %s\n' "$nfsdk_selected_major" "$nfsdk_selected_java"
