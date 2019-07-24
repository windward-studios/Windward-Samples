Contributing
============
The module is in restfulengine.py.  This is what you distribute, and this is
what you import.

This Python module uses:
* Requests for the HTTP requests
    http://docs.python-requests.org/en/latest/
    $ pip install requests
* py.test for testing
    http://pytest.org/latest/
    $ pip install pytest
* Sphinx for documentation
    http://sphinx-doc.org/
    $ pip install sphinx
    
Compatibility (Python 2 and Python 3)
-------------------------------------
This module is compatible with either Python 2 (tested on 2.7) or 3
(tested on 3.4) with one exception.  For Python 2, an additional module
is required: enum (https://pypi.python.org/pypi/enum/)

TESTING
-------
Tests are in tests/test_pythonclient.py. They use py.test. Once before running,
you must run the following:

    $ pip install -e .

From the project's top level directory (PythonClient).  To run the tests, run

    $ py.test

which will collect all the tests and run them.  These tests do expect a
RESTfulEngine instance running on port 49731 on localhost.  More info on
pytest can be found on the py.test website at pytest.org.  The tests mimic
the original tests in the CSharp client.


DOCUMENTATION
-------------
To generate documentation from the source files, run:

    $ make html


PyPI distribution
-----------------
See http://peterdowns.com/posts/first-time-with-pypi.html

Note: You need a HOME environment variable pointing to a location that contains
a .pypirc file.

setup.py is already set up, but before pushing to pypi, you'll want to open setup.py and change the version number.
 
With the version number changed, you simply have to run `python setup.py sdist upload -r pypi`

To upload documentation, go to edit the package at:
https://pypi.python.org/pypi?%3Aaction=pkg_edit&name=restfulengine


FILE INFORMATION
----------------
Sphinx generates the following directories:
    _build, _static and _templates

The distutils stuff generates the following directories:
    restfulengine.egg-info

tests directory contains all the tests and their associated data.

conf.py is for Sphinx.
index.rst is for Sphinx
make.bat is for Sphinx
Makefile is for Sphinx
restfulengine.py is the module
sandbox.py is just a sandbox file. You can ignore it or delete it or play in it.
setup.py is for distutils (and useful for uploading to PyPI)