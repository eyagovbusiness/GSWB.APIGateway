#!/usr/bin/dumb-init /bin/sh
# This script offers functions to wait for a service to become available.

set -e
# set -x

# Args: host, port, retries(optional)
# This function awaits until the provided host service from the first argument is running on the specified port
# This function can be used during a container entrypoint to wait until a certain service(local or forom another container) is up.
wait_for() {
  local host="$1"
  local port="$2"
  local retries="${3:-90}" # Number of retries, default is 90
  #local cmd="$2"

  count=0
  echo "Waiting for $host service to be available at $host:$port..."
  until nc -z "$host" "$port" >/dev/null 2>&1 || [ $((count++)) -gt $retries ]; do
    sleep 2
  done

  if [ "$count" -gt $retries ]; then
    echo "Error: $host service was not available after $retries every 2 seconds. Exiting..."
    exit 1
  else
    echo "$host service is available at $host:$port"
    #exec ${cmd}
  fi
}

# Args:port, retries(optional), wait_secs
# Check the health endpoint of the ASP.NET application running in this container(localhost). If not up after the max number of retries, exit the script with an error.
wait_for_health_endpoint() {
    local port=${1:-8080}     # Port, default is 8080
    local retries=${2:-30}  # Number of retries, default is 30
    local wait_secs=${3:-2} # Time in seconds between retries, default is 2

    for i in $(seq 1 "$retries"); do
        local response=$(echo -e "GET /health HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n" | nc -v -i 1 localhost "$port" | grep "200 OK" || true)

        # If the endpoint is up, print a message and return.
        if [[ -n "$response" ]]; then
            echo "/health endpoint is up."
            return
        fi

        echo "Waiting for /health endpoint to be up (Attempt $i/$retries)..."
        sleep "$wait_secs"
    done

    echo "Health endpoint not up after $retries attempts. Exiting."
    exit 1
}


# Args: host, port(optional), retries(optional)
# This function awaits until the provided host service from the first argument runs the IsReadyServer.sh script on the specified port.
# This function can be called during a container entrypoint that depends on another container to wait for it being ready before continuing with the entrypoint execution.
wait_IsReady() {
	local host="$1"
	local port="${2:-8000}"
	local retries="${3:-90}" # Number of retries, default is 90
	echo "Waiting for $host service to be ready..."
	RESPONSE=""
	count=0
	
	while [[ "$RESPONSE" != *"READY"* ]]; do
	  RESPONSE=$(nc "$host" "$port" || true)
	  sleep 2
	  count=$((count+1))
	  if [[ $count -ge $retries ]]; then
	    echo "Error: $host service is not ready after $count retries"
	    exit 1
	  fi
	done
	echo "$host service is ready!!"
}


