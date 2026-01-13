import { useState, useCallback, useEffect } from 'react';
import { AxiosError } from 'axios';

interface UseApiOptions {
  onSuccess?: (data: any) => void;
  onError?: (error: any) => void;
}

export function useApi<T = any>(
  apiCall: () => Promise<any>,
  options?: UseApiOptions
) {
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const execute = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await apiCall();
      const responseData = response.data;
      setData(responseData);
      options?.onSuccess?.(responseData);
      return responseData;
    } catch (err) {
      const axiosError = err as AxiosError<any>;
      const errorMessage = axiosError.response?.data?.message || axiosError.message || 'An error occurred';
      setError(errorMessage);
      options?.onError?.(err);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, [apiCall, options]);

  return {
    data,
    error,
    isLoading,
    execute,
    reset: () => {
      setData(null);
      setError(null);
    },
  };
}

export function useApiQuery<T = any>(
  apiCall: () => Promise<any>,
  options?: UseApiOptions & { enabled?: boolean }
) {
  const [data, setData] = useState<T | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(options?.enabled !== false);

  const refetch = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await apiCall();
      const responseData = response.data;
      setData(responseData);
      options?.onSuccess?.(responseData);
      return responseData;
    } catch (err) {
      const axiosError = err as AxiosError<any>;
      const errorMessage = axiosError.response?.data?.message || axiosError.message || 'An error occurred';
      setError(errorMessage);
      options?.onError?.(err);
    } finally {
      setIsLoading(false);
    }
  }, [apiCall, options]);

  // Auto-fetch on mount if enabled
  useEffect(() => {
    if (options?.enabled !== false) {
      refetch();
    }
  }, []);

  return {
    data,
    error,
    isLoading,
    refetch,
  };
}
