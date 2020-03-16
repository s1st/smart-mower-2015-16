#! /opt/local/bin/bash
pandoc -o "Dokumentation.pdf" -s -S --template=default.latex -V lang=ngerman -V documentclass=scrartcl "Dokumentation.md"
