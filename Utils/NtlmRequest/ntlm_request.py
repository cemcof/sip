import requests
import requests_ntlm
import sys
import argparse

arp = argparse.ArgumentParser(prog="ntlm_requests")

arp.add_argument('request_url')
arp.add_argument('-u', '--username')
arp.add_argument('-p', '--password')

arp = arp.parse_args()

if arp.request_url == 'stdin':
    arp.request_url = sys.stdin.read()

ntlm = requests_ntlm.HttpNtlmAuth(arp.username, arp.password)
result = requests.get(arp.request_url, auth=ntlm)

print(result.text)
