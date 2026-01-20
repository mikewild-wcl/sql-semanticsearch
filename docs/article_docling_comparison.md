# PDF chunking - a better way for our arXiv ingestion


## Python setup 

In VS Code, open the command palette and run Python: Create Environment, select an interpreter and let it create your environment in the .venv folder. Then activate it running Python: Select Interpreter from the command palette. You can also run the following command:
```
.venv/scripts/activate
```

Then in the terminal you can install ipykernel and create a new kernel - in this example I'm creating one for this project.

```
pip install ipykernel
python -m ipykernel install --user --name=semanticsearchkernelspec 
```

This will create a new kernel in your user Roaming folder - you can list it with
```
ls ${env:APPDATA}/jupyter/kernels/semanticsearchkernelspec
```

You should then be able to add this magic command to a polyglot notebook code cell 

```
#!connect jupyter --kernel-name semanticsearchkernelspec --kernel-spec semanticsearchkernelspec
```

Restart VS Code if it doesn't work first time.

Now when you create a new cell and click on the cell kernel you should see your new kernel and be able to run Python in your cell. Note that the magic command can only be run once in the notbook - it will show an error saying it's in use if you rerun it.
