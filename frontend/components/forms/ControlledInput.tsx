"use client";

import {
  Controller,
  type Control,
  type FieldValues,
  type FieldPath,
} from "react-hook-form";

type ControlledInputProps<T extends FieldValues> = {
  control: Control<T>;
  name: FieldPath<T>;
  label?: string;
  type?: string;
  placeholder?: string;
  autoComplete?: string;
};

export function ControlledInput<T extends FieldValues>({
  control,
  name,
  label,
  type = "text",
  placeholder,
  autoComplete,
}: ControlledInputProps<T>) {
  return (
    <Controller
      control={control}
      name={name}
      render={({ field, fieldState }) => (
        <div>
          {label && (
            <label className="block text-sm font-medium text-gray-700">
              {label}
            </label>
          )}
          <input
            {...field}
            type={type}
            placeholder={placeholder}
            autoComplete={autoComplete}
            className="mt-1 appearance-none relative block w-full px-3 py-2 border border-gray-300 placeholder-gray-500 text-gray-900 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
          />
          {fieldState.error && (
            <p className="mt-1 text-sm text-red-600">
              {fieldState.error.message}
            </p>
          )}
        </div>
      )}
    />
  );
}

export default ControlledInput;
