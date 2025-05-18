#!/usr/bin/env python

# This script is used to process semantic versioning

import json
import os
import subprocess
import sys

from shutil import which

def log(level: str, message: str) -> None:
    for message_line in message.splitlines():
        print(f"##[{level}]{message_line}")

def log_info(message: str):
    log('info', message)

def log_debug(message: str):
    log('debug', message)

def log_error(message: str):
    log('error', message)

def set_variable(name: str, value: str, is_output: bool = False, debug: bool = False) -> None:
    variable_definition = f"variable={name}"

    if is_output:
        variable_definition += ";isOutput=true"
    
    print(f"##vso[task.setvariable {variable_definition}]{value}")

    if debug:
        log_info(f"SetVariable: '{name}' = '{value}'")

def get_build_number() -> str | None:
    return os.getenv('BUILD_BUILDNUMBER')

def set_build_number(build_number: str) -> None:
    print(f"##vso[build.updatebuildnumber]{build_number}")

def run_gitversion(target_directory: str) -> dict:
    dotnet_executable = which("dotnet")
    gitversion_commandline = f"{dotnet_executable} gitversion {target_directory}"
    log_debug(f"Launch process: '{gitversion_commandline}'")

    (exitCode, output) = subprocess.getstatusoutput(gitversion_commandline)

    if exitCode == 0:
        log_debug("Process STDOUT:")
        log_debug(output)

        gitversion_variables: dict  = json.loads(output)

        return {
            variable_name: (variable_value or '')
            for (variable_name, variable_value) in gitversion_variables.items()
        }
    else:
        log_error("Process STDOUT:")
        log_error(output)

        raise Exception(f"'dotnet gitversion': process exit command ({exitCode}) does not indicate success.")

if __name__ == "__main__":
    args: list[str] = sys.argv
    if len(args) != 2:
        print(f"Usage: {args[0]}")

        exit(2)

    print(f"PATH = '{os.getenv("PATH")}'")

    target_dir = args[1]
    gitversion_variables = run_gitversion(args[1])

    print(
        json.dumps(gitversion_variables, indent=2, sort_keys=True)
    )

    # Odd-numbered minor versions are considered pre-release (this is due to long-standing limitations in the VS extension gallery's handling of package versions).
    minor_version = int(gitversion_variables["Minor"])
    if minor_version % 2 == 1:
        gitversion_variables["IsPreRelease"] = 'true'
    else:
        gitversion_variables["IsPreRelease"] = 'false'

    version_prefix = gitversion_variables["MajorMinorPatch"]

    build_metadata = gitversion_variables.get("BuildMetaData")
    if build_metadata:
        version_prefix = f"{version_prefix}.{build_metadata}"
    
    version_suffix = gitversion_variables.get('PreReleaseTag')
    
    gitversion_variables["VersionPrefix"] = version_prefix
    gitversion_variables["VersionSuffix"] = version_suffix

    for variable_name in gitversion_variables.keys():
        variable_value = gitversion_variables[variable_name]
        set_variable(variable_name, variable_value, is_output=True)

    semver = gitversion_variables["SemVer"]
    branch_name = gitversion_variables["EscapedBranchName"]
    commits_since_source_version = int(
        gitversion_variables.get("CommitsSinceVersionSource", "0")
    )

    set_build_number(f"{semver}+{branch_name}.{commits_since_source_version}")
