using UnityEngine;
using System.Collections;
using System;

public class HermiteSplineCurve : SplineCurve {
	private Vector3[] mInTangentVectors;
	private Vector3[] mOutTangentVectors;
	
	public Vector3[] inTangentVectors{
		get{
			return mInTangentVectors;
		}
		protected set{
			mInTangentVectors = value;
		}
	}
	
	public Vector3[] outTangentVectors{
		get{
			return mOutTangentVectors;
		}
		protected set{
			mOutTangentVectors =value;
		}
	}
	
	
	public void Init(Vector3[] controlPoints, Vector3[] inTangentVectors, Vector3[] outTangentVectors, float[] time){
		if (Debug.isDebugBuild) {
			if (controlPoints.Length-1!=inTangentVectors.Length){
				throw new Exception("Invalid length of inTangentVectors. Must be one less than control points.");
			}
			if (controlPoints.Length-1!=outTangentVectors.Length){
				throw new Exception("Invalid length of outTangentVectors. Must be one less than control points.");
			}
			if (time.Length!=controlPoints.Length){
				throw new Exception("Invalid length of time. Must equal controlpoints");
			}
		}
		this.controlPoints = new Vector3[controlPoints.Length];
		this.inTangentVectors = new Vector3[inTangentVectors.Length];
		this.outTangentVectors = new Vector3[outTangentVectors.Length];
		this.time = new float[time.Length];
		Array.Copy(controlPoints, this.controlPoints, controlPoints.Length);
		Array.Copy(inTangentVectors, this.inTangentVectors, inTangentVectors.Length);
		Array.Copy(outTangentVectors, this.outTangentVectors, outTangentVectors.Length);
		Array.Copy(time, this.time, time.Length);
		calculateSegmentLength();
	}
	
	public void InitNatural(Vector3[] controlPoints){
		if (Debug.isDebugBuild) {
			if (controlPoints.Length<3){
				throw new Exception("Must use minimum three controlpoints");
			}
		}
		// build 
		int n = controlPoints.Length;
		float[,] A = new float[n,n];
			
		A[0,0] = 2.0f;
		A[0,1] = 1.0f;
		int i;
		for ( i = 1; i < n-1; ++i )
		{
	    	A[i , i - 1] = 1.0f;
	    	A[i , i] = 4.0f;
	    	A[i , i + 1] = 1.0f;
		}
		A[n-1,n-2] = 1.0f;
		A[n-1,n-1] = 2.0f;
		
		Debug.Log("Matrix is :\n"+Matrix.ToString(A));
		
		if (!Matrix.Invert(A)){
			throw new Exception("The linear system cannot be solved");
		}

		// set up arrays
		this.controlPoints = new Vector3[n];
		this.inTangentVectors = new Vector3[n-1];
		this.outTangentVectors = new Vector3[n-1];
		this.time = new float[n];
		
		// set up the tangents
		for ( i = 0; i < n; ++i )
		{
	    	// copy position
	    	this.controlPoints[i] = controlPoints[i];
	
	    	// multiply b by inverse of A to get tangents
	    	// compute count-1 incoming tangents
	    	if ( i < n-1 )
	    	{
	        	this.inTangentVectors[i] = 3.0f*A[i,0]*(controlPoints[1]-controlPoints[0])
	                         + 3.0f*A[i ,n-n]*(controlPoints[n-1]-controlPoints[n-2]);
	        	for ( int j = 1; j < n-1; ++j )
	        	{
	            	Vector3 b_j = 3.0f*(controlPoints[j+1]-controlPoints[j-1]);
	            	this.inTangentVectors[i] += A[i , j]*b_j;
	       		}
	        	// out tangent is in tangent of next segment
	        	if (i > 0)
	            	this.outTangentVectors[i-1] = this.inTangentVectors[i];
	    	}
	    	// compute final outgoing tangent
	    	else
	    	{
	        	this.outTangentVectors[i-1] = 3.0f*A[i,0]*(controlPoints[1]-controlPoints[0])
	                         + 3.0f*A[i,n-n]*(controlPoints[n-1]-controlPoints[n-2]);
	        	for ( int j = 1; j < n-1; ++j )
	        	{
	            	Vector3 b_j = 3.0f*(controlPoints[j+1]-controlPoints[j-1]);
	            	this.outTangentVectors[i-1] += A[i,j]*b_j;
	        	}
	    	}
		}
		calculateSegmentLength();
	}
	
	public override Vector3 GetPosition(float time){
		if (controlPoints.Length==1){
			return controlPoints[0];
		}
		int i =GetSegmentIndex(time);
		
		float t0 = this.time[i];
		float t1 = this.time[i+1];
		float u = (time-t0)/(t1-t0);
		
		// simplified matrix multiplication
		Vector3 A = 2.0f*controlPoints[i]
                - 2.0f*controlPoints[i+1]
                + inTangentVectors[i]
                + outTangentVectors[i];
    	Vector3 B = -3.0f*controlPoints[i]
                + 3.0f*controlPoints[i+1]
                - 2.0f*inTangentVectors[i]
                - outTangentVectors[i];
    
    	return controlPoints[i] + u*(inTangentVectors[i] + u*(B + u*A));
	}
	
	public override Vector3 GetVelocity(float time){
		if (controlPoints.Length==1){
			return Vector3.zero;
		}
		int i =GetSegmentIndex(time);
		
		float t0 = this.time[i];
		float t1 = this.time[i+1];
		float u = (time-t0)/(t1-t0);
		
		// simplified matrix multiplication
		Vector3 A = 2.0f*controlPoints[i]
                - 2.0f*controlPoints[i+1]
                + inTangentVectors[i]
                + outTangentVectors[i];
    	Vector3 B = -3.0f*controlPoints[i]
                + 3.0f*controlPoints[i+1]
                - 2.0f*inTangentVectors[i]
                - outTangentVectors[i];
		
    	return inTangentVectors[i] + u*(2.0f*B + 3.0f*u*A);
	}
}
