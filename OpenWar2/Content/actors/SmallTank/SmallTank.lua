
if initialized == nil then
	initialized = true;
	
	-- Set some properties of the object
	SetSprite("ball");
	xBounciness = 0.9;
	yBounciness = 0.9;
	gravity = 0.02;
	
			-- Randomize vel
		math.randomseed( os.time() );
		math.random(); math.random(); math.random();
		
		xVel = (math.random() * 1) - 0.5;
		math.random(); math.random(); math.random();
		yVel = (math.random() * 1) - 0.5;
end