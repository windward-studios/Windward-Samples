__author__ = 'marcusj'

import requests
import base64
import json

url = "http://localhost:49731"
version = "/v1/version"
reports = "/v1/reports"
headers = {'Content-Type': 'application/json;charset=UTF-8', 'Accept': 'application/json'}
service_version_field = 'ServiceVersion'
engine_version_field = 'EngineVersion'


def getversion():
    r = requests.get(url+version, headers=headers)
    print(r.json()[service_version_field])
    print(r.json()[engine_version_field])
    return r


def postfile():
    f = open("tests/data/Sample1.docx", "rb")  # file
    b = f.read()  # bytestring
    e = base64.b64encode(b)  # Base64 encoded byte string
    data = e.decode("utf-8")  # String representation
    payload = {'Data': data,
               'OutputFormat': 'pdf'}
    r = requests.post(url+reports, headers=headers, data=json.dumps(payload))
    return r


def test(a, b, c):
    """

    :param a:
    :param b:
    :param c:
    :return:
    """
    print(a)
    print(b)
    print(c)

if __name__ == '__main__':
    test({"asdf": 1, "qwer": 2}, [1, "asdf", 3], "squaggle")