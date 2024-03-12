#!/bin/bash -ex

# Print disk status before cleaning
df -h

# We don't care about errors in this section, we just want to clean as much as possible
set +e

DIR="$(dirname "${BASH_SOURCE[0]}")"

"$DIR"/clean-xcodes.sh
"$DIR"/clean-simulator-runtimes.sh
"$DIR"/clean-logs.sh

# Print disk status after cleaning
df -h
