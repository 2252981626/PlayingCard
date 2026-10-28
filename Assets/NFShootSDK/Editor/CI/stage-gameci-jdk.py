#!/usr/bin/env python3
"""Stage an installed JDK without links back to the runner's /etc or tool cache."""
import argparse
import os
from pathlib import Path
import shutil
import sys


def stage(source, destination):
    source = Path(source).resolve(strict=True)
    workspace = Path(os.environ['GITHUB_WORKSPACE']).resolve(strict=True)
    requested = Path(os.path.abspath(destination))
    expected = workspace / '.ci-tools' / 'jdk'
    if (requested.name != 'jdk' or requested.parent.name != '.ci-tools' or
            requested.parent.parent.resolve() != workspace or requested.parent.is_symlink() or requested.is_symlink()):
        raise ValueError('Destination must be the workspace-owned .ci-tools/jdk directory, without directory symlinks.')
    destination = expected
    if source == destination or source in destination.parents or destination in source.parents:
        raise ValueError('The source JDK and staged JDK directories must not overlap.')

    source_cacerts = source / 'lib/security/cacerts'
    if not source_cacerts.is_file() or not os.access(source_cacerts, os.R_OK) or source_cacerts.stat().st_size == 0:
        raise ValueError('Source JDK cacerts is missing, empty or unreadable: ' + str(source_cacerts))

    def optional_broken_sources(directory, names):
        # Linux packages can link to optional sources that are not installed.
        # Other broken links, including runtime configuration, must fail copying.
        ignored = []
        for name in names:
            path = Path(directory) / name
            relative = path.relative_to(source).as_posix()
            if relative in ('src.zip', 'lib/src.zip') and path.is_symlink() and not path.exists():
                ignored.append(name)
        return ignored

    # Remove only our generated destination. Copying over an old external symlink
    # could otherwise write through that link into a host-owned file.
    if destination.exists():
        shutil.rmtree(destination)
    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copytree(source, destination, symlinks=False, ignore=optional_broken_sources)

    cacerts = destination / 'lib/security/cacerts'
    if cacerts.is_symlink() or not cacerts.is_file() or not os.access(cacerts, os.R_OK) or cacerts.stat().st_size == 0:
        raise ValueError('Staged JDK must contain a readable regular cacerts file: ' + str(cacerts))
    print('Staged JDK with materialized runtime files and cacerts: ' + str(destination))


if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--source', required=True)
    parser.add_argument('--destination', required=True)
    args = parser.parse_args()
    try:
        stage(args.source, args.destination)
    except (OSError, ValueError, shutil.Error) as error:
        print('::error::JDK staging failed: ' + str(error), file=sys.stderr)
        sys.exit(1)
