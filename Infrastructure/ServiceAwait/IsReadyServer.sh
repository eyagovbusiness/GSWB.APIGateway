# This script acts like a server that listens on the port specified in the first argument or defaults to 8000. It responds to any request with an HTTP format with code 200 and a basic HTML containing only the word 'READY'
# This script can be used during Docker entrypoints to declare the service as ready to dependent containers. By running this server and executing the script, dependent containers can establish a connection to check if the service they depend on is ready or not.
# (See 'wait_for_service.sh -> function wait_IsReady()' for an awaiter function sample.)
# IMPORTANT: Dont forget to expose the port this server will use in the Dockerfile!!
set -e
#set -x

echo "IsReadyServer up, other dependent services can start/continue!"
port=${1:-8000}
while true; do
	echo -e 'HTTP/1.1 200\r\nContent-Type: text/html\r\n\r\n<h1>READY</h1>' | nc -lv -p "$port"
done