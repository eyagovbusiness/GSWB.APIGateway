import requests
import base64
import os

#CONSTANTS
username            = os.environ.get('USER')
token               = os.environ.get('TOKEN')
env                 = os.environ.get('ENV')
url_base            = 'https://jenkins.guildswarm.org'
url_crumb           = f'{url_base}/crumbIssuer/api/json'
url_in_progress     = f'{url_base}/job/GuildSwarm/job/Auto/job/Backend/job/backend_deploy_stg/job/integration/lastBuild/api/json'
url_jobs            = f'{url_base}/job/GuildSwarm/job/Auto/job/Backend/job/backend_deploy_stg/job/integration/build'

#job Builder
def jobBuild(headersRequest):
        try:
            jobBuildResponse=requests.post(url_jobs, headers=headersRequest)
            print(jobBuildResponse)
        except requests.exceptions.RequestException as e:
            print(f"Error: {e}")

# Get Crumb issuer
def getCrumb(username, token):
    credentials = f'{username}:{token}'
    base64_credentials = base64.b64encode(credentials.encode('utf-8')).decode('utf-8')
    headers = {
        'Authorization': f'Basic {base64_credentials}',
        'Content-Type': 'application/json'
    }
    try:
        responseCrumb = requests.get(url_crumb, headers=headers)
        data = responseCrumb.json()
        crumbField = data.get("crumb")
        return crumbField, base64_credentials
    except requests.exceptions.RequestException as e:
        print(f"Error: {e}")
        return None, None
    
# Main :)
def main():
    crumb, creds=getCrumb(username, token)
    headersRequest = {
        'Authorization': f'Basic {creds}',
        'Jenkins-Crumb': f'{crumb}',
        'Content-Type' : 'application/json'
    }
    try:
        responseRequest = requests.get(url_in_progress, headers=headersRequest)
        dataDict = responseRequest.json()
        in_progress = dataDict.get('inProgress')
        if in_progress != False:
            print("Job is building skipping to send the build")
        else:
            print("Sending Build request")
            jobBuild(headersRequest)
    except requests.exceptions.RequestException as e:
        print(f"Error: {e}")

main()