import os
import sys
import zipfile
import json

bin = os.path.abspath(os.path.join(os.path.dirname(__file__), os.pardir, "MintTea", "bin", "Debug", "MintTea.dll"))
manifest = os.path.abspath(os.path.join(os.path.dirname(__file__), "manifest.json"))
icon = os.path.abspath(os.path.join(os.path.dirname(__file__), "icon.png"))
readme = os.path.abspath(os.path.join(os.path.dirname(__file__), "README.md"))

source = os.path.join(os.path.dirname(__file__), os.pardir, "MintTea/MintTea.cs")


def clean():
    path = os.path.dirname(os.path.abspath(__file__))
    for file in os.listdir(path):
        if os.path.isfile(os.path.join(path, file)) and file[-4:] == ".zip":
            os.remove(file)

def getOutFile(version):
    return os.path.join(os.path.dirname(__file__), "MintTea-" + version + ".zip")

def assertBin(version):
    with open(bin, "rb") as file:
        return version in str(file.read())
    return False

def assertSource(version):
    with open(source, "rb") as file:
        return version in str(file.read())
    return True

def updateManifest(version):
    with open(manifest, "r") as file:
        content = json.loads(file.read())
        content["version_number"] = version
    with open(manifest, "w") as file:
        file.write(json.dumps(content, indent = 4))
        return True
    return False

def assertReadme(version):
    with open(readme, "r") as file:
        return version in str(file.read())
    return False


def getVersion():
    assert len(sys.argv) == 2
    return sys.argv[1]

def pack(version):
    with zipfile.ZipFile(getOutFile(version), "w", zipfile.ZIP_DEFLATED) as handle:
        handle.write(bin, os.path.split(bin)[1])
        handle.write(manifest, os.path.split(manifest)[1])
        handle.write(icon, os.path.split(icon)[1])
        handle.write(readme, os.path.split(readme)[1])

def main():

    clean()

    version = getVersion()

    shouldRelease = True
    b = assertBin(version) 
    if not b:
        print("Wrong binary")
    shouldRelease = shouldRelease and b
    b = assertSource(version)
    if not b:
        print("Wrong source")
    shouldRelease = shouldRelease and b
    b = updateManifest(version)
    if not b:
        print("Wrong manifest")
    shouldRelease = shouldRelease and b
    if not assertReadme(version):
        print("No changelog in readme!!")

    if shouldRelease:
        pack(version)
        os.system("git commit -am \"Release " + version + "\"")
        os.system("git tag v" + version)
        os.system("git push origin master v" + version)

if __name__ == "__main__":
    main()