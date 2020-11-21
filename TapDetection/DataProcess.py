# suppose to preprocess the data from GT (sensor morph) and raw input (from tap strap device)
# annotate each item in raw input
# considering evenly distribute the data from raw input (TODO)

import pickle
import re
import numpy as np
import math

fRawInput = []
fGroundTruth = []

with open("C:\\Projects\\tap-standalonewin-sdk\\TAPWinApp\\bin\\Debug\\fingers1605155844211.txt", 'r') as fd:
    for line in fd:
        fRawInput.append(line)

with open("C:\\Projects\\sensel-api\\sensel-examples\\sensel-c\\example-4-multi\\build\\x64\\Release\\11111136.txt", 'r') as fd:
    for line in fd:
        fGroundTruth.append(line)

 
# data format
# timestamp, 3 floats x 5 fingers, contact or not for each finger
# TODO timestamp, 3 floats x 5 fingers, one int represents which finger is down, -1 means no contacts
GTlen = len(fGroundTruth)
GTindex = 0
rawInputLen = len(fRawInput)
x_data = np.zeros([rawInputLen, 15], dtype=float) # 3 floats x 5 fingers
y_data = np.zeros([rawInputLen, 1], dtype=int) # one int to specify 5 fingers

fingerMap = {
    "q":0,
    "3":1,
    "4":2,
    "t":3,
    "b":4
}
delimter = ",|\t| |\n"
for index, item in enumerate(fRawInput):
#     find D[own]
    while GTindex < GTlen:
        curGT = fGroundTruth[GTindex]
        curGTTS = re.split(delimter, curGT)[0]
        curGTOp = re.split(delimter, curGT)[2]
        curGTFinger = re.split(delimter, curGT)[3]
        if curGTOp == "D":
            break
        else:
            GTindex = GTindex + 1

    # insert raw data
    curRawData = re.split(delimter, item)
    curTS = curRawData[0]
    x_data[index] = np.array(curRawData[4:7] + curRawData[10:13] + curRawData[16:19] + curRawData[22:25] + curRawData[28:31])

    # q34tb
    if GTindex < GTlen and int(curGTTS) < int(curTS):
        # check if cur or prev one should be annotated based on GT
        print("process with GT ", GTindex, " and raw data ", index)
        if abs(int(re.split(delimter, fRawInput[index-1])[0]) - int(curGTTS)) < abs(int(curTS) - int(curGTTS)):
            # prev
            # y_data[index - 1][fingerMap[re.split(delimter, fGroundTruth[GTindex - 1])[3]]] = 1
            y_data[index - 1] = fingerMap[re.split(delimter, fGroundTruth[GTindex - 1])[3]]+1
        else:
            # y_data[index][fingerMap[re.split(delimter, fGroundTruth[GTindex])[3]]] = 1
            y_data[index] = fingerMap[re.split(delimter, fGroundTruth[GTindex])[3]]+1
        GTindex = GTindex+1

print("train", math.floor(rawInputLen*0.8/64)*64)
print("valid", math.floor(rawInputLen/64)*64 - math.floor(rawInputLen*0.8/64)*64)

pickle.dump( x_data, open( "x_data.pkl", "wb" ) )
pickle.dump( y_data, open( "y_data.pkl", "wb" ) )
pickle.dump( x_data[0:math.floor(rawInputLen*0.8/64)*64], open( "x_train.pkl", "wb" ) )
pickle.dump( y_data[0:math.floor(rawInputLen*0.8/64)*64], open( "y_train.pkl", "wb" ) )
pickle.dump( x_data[math.floor(rawInputLen*0.8/64)*64:math.floor(rawInputLen/64)*64], open( "x_valid.pkl", "wb" ) )
pickle.dump( y_data[math.floor(rawInputLen*0.8/64)*64:math.floor(rawInputLen/64)*64], open( "y_valid.pkl", "wb" ) )
