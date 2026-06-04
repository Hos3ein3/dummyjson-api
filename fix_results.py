import os
import glob

def process_file(filepath):
    with open(filepath, 'r') as f:
        content = f.read()

    new_content = content.replace('using DummyJson.Domain.Common.Models;', 'using SharedKernel.Results;')
    new_content = new_content.replace('DomainError.NotFound', 'CommonErrors.NotFound')
    new_content = new_content.replace('DomainError.Validation', 'Error.Validation')
    new_content = new_content.replace('DomainError.Conflict', 'CommonErrors.Duplicate') # or Conflict
    new_content = new_content.replace('DomainError.Unauthorized', 'CommonErrors.Unauthorized')
    new_content = new_content.replace('DomainError.Forbidden', 'CommonErrors.Forbidden')
    new_content = new_content.replace('DomainError.None', 'Error.None')
    new_content = new_content.replace('DomainError.NullValue', 'CommonErrors.Unexpected("A null value was provided.")')
    new_content = new_content.replace('DomainError', 'Error')

    if new_content != content:
        with open(filepath, 'w') as f:
            f.write(new_content)
        print(f"Updated {filepath}")

for root, _, files in os.walk('src'):
    for file in files:
        if file.endswith('.cs'):
            process_file(os.path.join(root, file))
