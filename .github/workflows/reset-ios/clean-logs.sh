#!/bin/bash -eux

if [ -f "$HOME/Library/Logs/CoreSimulator/*" ]; then rm -r $HOME/Library/Logs/CoreSimulator/*; fi

if [ -f "$HOME/Library/Logs/DiagnosticReports/*" ]; then rm -r $HOME/Library/Logs/DiagnosticReports/*; fi
