import xml.etree.ElementTree as ET
import json
import sys

test_cases = []
with open('test_cases.json', 'r') as f:
    test_cases = json.load(f)['tests']

tests_dict = dict()
for test in test_cases:
    tests_dict[test['fullname']] = test


try:
    tree = ET.parse('artifacts/playmode-results.xml')
except FileNotFoundError:
    print('BUILD FAILED! Please check previous step for details. ' +
          '(Did you set up your Unity secrets for this repository?)')
    sys.exit(1)

root = tree.getroot()

score = 0
max_score = 0

for suite in root.iter('test-suite'):
    name = suite.attrib['fullname']
    if name in tests_dict:
        suite_result = suite.attrib['result']
        tests_dict[name]['result'] = suite_result
        tests_dict[name]['info'] = []
        for case in suite.iter('test-case'):
            tests_dict[name]['info'].append(case)

# Process in order of .json file
for test_case in test_cases:
    test = tests_dict[test_case['fullname']]
    max_score += test['points']
    if 'result' not in test:
        print(f'Expected test {test_case["fullname"]} but was not found in results.xml')
        continue
    result = test['result']

    items = test['info']
    test_points = test['points']
    earned_points = 0
    message = ''
    name = test['fullname']
    if result not in ['Passed', 'Failed']:
        print(f'Unrecognized result type "{result}" for test "{name}"')
    if result == 'Passed':
        score += test_points
        earned_points = test_points
    print(f'{name}: {result} ({earned_points}/{test_points})')
    for item in items:
        output = item.find('output')
        failure = item.find('failure')
        if output is not None or failure is not None:
            print("-------------")
            print(f"{item.attrib['name']}{'' if failure is None else ' FAILED'}:")
        if result == 'Failed':
            if failure is not None:
                message = failure.find('message').text
                stack_trace = failure.find('stack-trace').text
                print(f'Failure message:\n{message}')
                print(f'Stack trace:\n{stack_trace}')
        if output != None:
            print(f'Test Output:\n{output.text}')

print(f'Total: {score}/{max_score}')

exit_code = 0 if score == max_score and score != 0 else 1
sys.exit(exit_code)
