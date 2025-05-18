#!/usr/bin/env python

# This script is used to process semantic versioning

import json
import os
import sys

from subprocess import run, CompletedProcess

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

def run_gitversion(target_directory: str = ".") -> dict:
    gitversion_process: CompletedProcess = run(["dotnet", "gitversion", target_directory], shell=True, capture_output=True)

    process_stderr: bytes = gitversion_process.stderr
    process_stdout: bytes = gitversion_process.stdout

    if process_stdout:
        log_debug("Process STDOUT:")
        log_debug(
            process_stdout.decode("utf-8")
        )

    if process_stderr:
        log_debug("Process STDERR:")
        log_debug(
            process_stdout.decode("utf-8")
        )

    if gitversion_process.returncode == 0:
        if process_stdout:
            log_debug("Process STDOUT:")
            log_debug(
                process_stdout.decode("utf-8")
            )

        if process_stderr:
            log_debug("Process STDERR:")
            log_debug(
                process_stdout.decode("utf-8")
            )

        gitversion_variables: dict  = json.loads(process_stdout)

        return {
            variable_name: (variable_value or '')
            for (variable_name, variable_value) in gitversion_variables.items()
        }

    if gitversion_process.returncode != 0:
        if (process_stderr):
            log_error(
                process_stderr.decode("utf-8")
            )
        
        if (process_stdout):
            log_error(
                process_stdout.decode("utf-8")
            )

        raise Exception(f"'dotnet gitversion': process exit command ({gitversion_process.returncode}) does not indicate success.")

if __name__ == "__main__":
    args: list[str] = sys.argv
    if len(args) != 2:
        print(f"Usage: {args[0]}")

        exit(2)

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

    full_semver = gitversion_variables['FullSemVer']

    set_build_number(full_semver)
