#!/usr/bin/env ipy

import clr
clr.AddReference("MyMediaLite.dll")
clr.AddReference("MyMediaLiteExperimental.dll")
from MyMediaLite import *

min_rating = 0;
max_rating = 100;

train_file = "trainIdx2.firstLines.txt"
test_file  = "testIdx2.firstLines.txt"

# load the data
train_data = IO.KDDCup2011.RatingData.Read(train_file)
test_data = IO.KDDCup2011.Track2Candidates.Read(test_file)

# set up the recommender
recommender = RatingPrediction.UserItemBaseline()
recommender.MinRating = min_rating
recommender.MaxRating = max_rating
recommender.Ratings = train_data
print "Training ..."
recommender.Train()
print "done."

# measure the accuracy on the test data set
#print Eval.RatingEval.Evaluate(recommender, test_data)

for user_id in test_data.Keys:
	for item_id in test_data[user_id]:
		print repr(user_id) + "\t" + repr(item_id) + "\t" + repr(recommender.Predict(user_id, item_id))

