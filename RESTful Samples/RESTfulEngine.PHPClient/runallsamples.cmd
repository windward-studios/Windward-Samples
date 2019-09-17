@echo off

rem run all samples simultaneously
rem each sample, except sample1.php generates a report

for %%f in (sample?.php) do start cmd /k php %%f
