#!/bin/bash

# This script acts as a custom entrypoint that performs custom operations before and after starting a base Members ASP.NET service.

# Exit immediately if a command exits with a non-zero status.
set -e
#set -x

# "import" service await functions
source wait_for_service.sh

# Execute tasks before the base entrypoint starts
execute_before_start() {
    echo "Executing scheduled tasks before the base entrypoint starts.."	
	wait_IsReady vault.vault.svc.cluster.local #vault waits consul is ready :)
	echo "Scheduled tasks before the base entrypoint starts..DONE."
}

# Main function that orchestrates the execution of the script, executing first the function with the custom logic that should be executed before calling the base entrypoint. 
# Secondly calls the base entrypoint and finally executed the function with the custom logic that should be executed after the base entripoint is called.
main() {
    echo "Starting entrypoint override" 
    execute_before_start

    echo "Calling base Entrypoint"
    #dotnet APIGateway.API.dll &
	./APIGateway.API &
    local baseentry_pid=$!
	
    wait "$baseentry_pid" 
    echo "Entrypoint override exited"
}

# Execute the main function.
main "$@"