try:
    from setuptools import setup
except ImportError:
    from distutils.core import setup

config = {
    'name': 'restfulengine',
    'version': '1.0',
    'description': 'Python client for the Windward RESTful Engine',
    'long_description': '',
    'url': 'http://www.windward.net/products/restful/',
    'author': 'Windward Studios',
    'author_email': 'support@windward.net',
    'install_requires': ['requests'],
    'py_modules': ['restfulengine']
}

setup(**config)