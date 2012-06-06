@echo off

pushd ..

assoc .ix=Nine.Project
ftype "Nine.Project"="%CD%\Bin\Engine Nine.exe" %%1

popd