import { HttpInterceptorFn } from '@angular/common/http';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const token = localStorage.getItem('jwt_token');

  // If a token exists, clone the request and attach the header
  if (token) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    
    // Pass the cloned request to the next handler
    return next(authReq);
  }

  // If no token exists (e.g., during login), send the request as-is
  return next(req);
};