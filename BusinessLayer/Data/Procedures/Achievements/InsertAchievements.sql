CREATE OR ALTER PROCEDURE InsertAchievements
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO Achievements (achievement_name, description, achievement_type, points, icon_url) 
    VALUES
    ('FRIENDSHIP1', 'You made a friend, you get a point', 'Friendships', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('FRIENDSHIP2', 'You made 5 friends, you get 3 points', 'Friendships', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('FRIENDSHIP3', 'You made 10 friends, you get 5 points', 'Friendships', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('FRIENDSHIP4', 'You made 50 friends, you get 10 points', 'Friendships', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('FRIENDSHIP5', 'You made 100 friends, you get 15 points', 'Friendships', 15, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('OWNEDGAMES1', 'You own 1 game, you get 1 point', 'Owned Games', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('OWNEDGAMES2', 'You own 5 games, you get 3 points', 'Owned Games', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('OWNEDGAMES3', 'You own 10 games, you get 5 points', 'Owned Games', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('OWNEDGAMES4', 'You own 50 games, you get 10 points', 'Owned Games', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('SOLDGAMES1', 'You sold 1 game, you get 1 point', 'Sold Games', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('SOLDGAMES2', 'You sold 5 games, you get 3 points', 'Sold Games', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('SOLDGAMES3', 'You sold 10 games, you get 5 points', 'Sold Games', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('SOLDGAMES4', 'You sold 50 games, you get 10 points', 'Sold Games', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEW1', 'You gave 1 review, you get 1 point', 'Number of Reviews Given', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEW2', 'You gave 5 reviews, you get 3 points', 'Number of Reviews Given', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEW3', 'You gave 10 reviews, you get 5 points', 'Number of Reviews Given', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEW4', 'You gave 50 reviews, you get 10 points', 'Number of Reviews Given', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEWR1', 'You got 1 review, you get 1 point', 'Number of Reviews Received', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEWR2', 'You got 5 reviews, you get 3 points', 'Number of Reviews Received', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEWR3', 'You got 10 reviews, you get 5 points', 'Number of Reviews Received', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
    ('REVIEWR4', 'You got 50 reviews, you get 10 points', 'Number of Reviews Received', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
	('DEVELOPER', 'You are a developer, you get 10 points', 'Developer', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
	('ACTIVITY1', 'You have been active for 1 year, you get 1 point', 'Years of Activity', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
	('ACTIVITY2', 'You have been active for 2 years, you get 3 points', 'Years of Activity', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
	('ACTIVITY3', 'You have been active for 3 years, you get 5 points', 'Years of Activity', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
	('ACTIVITY4', 'You have been active for 4 years, you get 10 points', 'Years of Activity', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
	('POSTS1', 'You have made 1 post, you get 1 point', 'Number of Posts', 1, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
	('POSTS2', 'You have made 5 posts, you get 3 points', 'Number of Posts', 3, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
	('POSTS3', 'You have made 10 posts, you get 5 points', 'Number of Posts', 5, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png'),
	('POSTS4', 'You have made 50 posts, you get 10 points', 'Number of Posts', 10, 'https://cdn-icons-png.flaticon.com/512/5139/5139999.png')
	;
END